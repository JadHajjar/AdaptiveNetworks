namespace AdaptiveRoads.Util; 
using AdaptiveRoads.Manager;
using AdaptiveRoads.UI;
using ColossalFramework;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static KianCommons.Helpers;

internal static class RoadUtils {
    public static List<string> GatherEditNames() {
        var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
        if (ground == null)
            throw new Exception("m_editPrefabInfo is not netInfo");
        NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
        NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
        NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
        NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

        var ret = GatherNetInfoNames(ground);
        ret.AddRange(GatherNetInfoNames(elevated));
        ret.AddRange(GatherNetInfoNames(bridge));
        ret.AddRange(GatherNetInfoNames(slope));
        ret.AddRange(GatherNetInfoNames(tunnel));

        return ret;
    }

    public static List<string> GatherNetInfoNames(NetInfo info) {
        var ret = new List<string>();
        if (!info) return ret;
        ret.Add(info.name);
        var ai = info.m_netAI;
        foreach (FieldInfo field in ai.GetType().GetFields()) {
            if (field.GetValue(ai) is BuildingInfo buildingInfo) {
                ret.Add(buildingInfo.name);
            }
        }
        return ret;
    }

    public static List<string> RenameEditNet(string name, bool reportOnly) {
        try {
            if (name.IsNullOrWhiteSpace())
                throw new Exception("name is empty");
            var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            if (ground == null)
                throw new Exception("m_editPrefabInfo is not netInfo");

            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            var ret = new List<string>();
            void Rename(NetInfo _info, string _postfix) {
                if (!_info) return;
                ret.Add(GetUniqueNetInfoName(name + _postfix, true));
                if (!reportOnly) _info.name = ret.Last();
                ret.AddRange(_info.NameAIBuildings(ret.Last(), reportOnly));
            }

            Rename(ground, "_Data");
            Rename(elevated, " E");
            Rename(bridge, " B");
            Rename(slope, " S");
            Rename(tunnel, " T");

            return ret;
        } catch (Exception ex) {
            Log.Exception(ex);
            return null;
        }
    }

    /// <summary>
    /// generates unique name by adding prefix.
    /// </summary>
    /// <param name="excludeCurrent">
    /// set to true when renaming
    /// set to false when cloning.
    /// </param>
    public static string GetUniqueNetInfoName(string name, bool excludeCurrent = false) {
        string strippedName = PackageHelper.StripName(name);
        if (excludeCurrent && strippedName == name)
            return name;
        string finalName = strippedName;
        for (int i = 0; PrefabCollection<NetInfo>.LoadedExists(finalName); i++) {
            finalName = $"instance{i}." + strippedName;
            if (i > 1000) throw new Exception("Infinite loop");
        }
        return finalName;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0101:Array allocation for params parameter", Justification = "<Pending>")]
    public static List<string> NameAIBuildings(this NetInfo info, string infoName, bool reportOnly) {
        var ret = new List<string>();
        var ai = info.m_netAI;
        string name = PackageHelper.StripName(infoName);
        foreach (FieldInfo field in ai.GetType().GetFields()) {
            if (field.GetValue(ai) is BuildingInfo buildingInfo) {
                string postfix = field.Name.Remove("m_", "Info");
                ret.Add(name + "_" + postfix);
                if (!reportOnly) {
                    buildingInfo.name = ret.Last();
                    Log.Debug($"set {info}.netAI.{field.Name}={buildingInfo.name}");
                }
            }
        }
        return ret;
    }

    public static void SetDirection(bool lht) {
        SimulationManager.instance.AddAction(() => SetDirectionImpl(lht));
    }
    public static void SetDirectionImpl(bool lht) {
        Log.Called();
        if(lht == NetUtil.LHT) return; // no need for change.
        SimulationManager.instance.m_metaData.m_invertTraffic =
            lht ? SimulationMetaData.MetaBool.True: SimulationMetaData.MetaBool.False;
        
        for(ushort i=0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
            var info = PrefabCollection<NetInfo>.GetLoaded(i);
            if(!info) continue;
            foreach(var lane in info.m_lanes) {
                const NetInfo.LaneType flags = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Parking | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle;
                if(lht && lane.m_laneType.IsFlagSet(flags)) {
                    lane.m_finalDirection = NetInfo.InvertDirection(lane.m_direction);
                } else {
                    lane.m_finalDirection = lane.m_direction;
                }
            }
            Swap(ref info.m_hasForwardVehicleLanes, ref info.m_hasBackwardVehicleLanes);
            Swap(ref info.m_forwardVehicleLaneCount, ref info.m_backwardVehicleLaneCount);
        }

        SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(RoadEditorUtils.RefreshRoadEditor);

        for(ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
            if(NetUtil.IsSegmentValid(segmentID)) {
                NetManager.instance.UpdateSegment(segmentID);
            }
        }
    }

    public static void SetTiling(this Material material, float tiling) {
        if (material) {
            material.mainTextureScale = new Vector2(1, tiling);
            // not sure if checksum changes if I change texture scale.to make sure checksum changes I also change the name.
            material.name = "NetworkTiling " + tiling.ToString("R");
        }
    }

    public static void FixRenderQueue(this Material material) {
        string shader = material?.shader?.name;
        if (shader == "Custom/Net/TrainBridge") {
            material.renderQueue = 2470;
        } else if (shader == "Custom/Net/Electricity") {
            // game sets it to 3000 by default.
            //material.renderQueue = 3000;
        }
    }

    public static bool WireHasWind(this NetInfo info) {
        foreach(var segmentInfo in info.m_segments) {
            string shader = segmentInfo.m_material.shader?.name;
            if (shader == "Custom/Net/Electricity") {
                foreach(var color in segmentInfo.m_mesh.colors) {
                    if(color != Color.black) {
                        return true;
                    }
                }
                return false;
            }
        }
        return false;
    }
    public static void SetupThinWires(bool force = false) {
        Log.Called();
        if (Helpers.InStartupMenu) return;
        if(force || ModSettings.ThinWires) {
            float scale = ModSettings.ThinWires ? ModSettings.WireScale : 1f;
            Log.Info("Thin wires: setting global wire width to " + scale);
            foreach (var netInfo in NetUtil.IterateLoadedNetInfos()) {
                netInfo?.SetupThinWires(scalex: scale);
            }
        }
    }

    public static void SetupThinWires(this NetInfoExtionsion.Track track, float scalex) {
        if (track == null || !track.m_requireWindSpeed) return; // not wire
        Vector2 scale = new Vector2(scalex, 1.0f);
        if (track.m_material) track.m_material.mainTextureScale = scale;
        if (track.m_trackMaterial) track.m_trackMaterial.mainTextureScale = scale;
        if (track.m_lodMaterial) track.m_lodMaterial.mainTextureScale = scale;
        if (track.m_combinedLod?.m_material) track.m_combinedLod.m_material.mainTextureScale = scale;
    }

    public static void SetupThinWires(this NetInfo.Segment segment, float salex) {
        if (segment == null || !segment.m_requireWindSpeed) return; // not wire
        Vector2 scale = new Vector2(salex, 1.0f);
        if (segment.m_material) segment.m_material.mainTextureScale = scale;
        if (segment.m_segmentMaterial) segment.m_segmentMaterial.mainTextureScale = scale;
        if (segment.m_lodMaterial) segment.m_lodMaterial.mainTextureScale = scale;
        if (segment.m_combinedLod?.m_material) segment.m_combinedLod.m_material.mainTextureScale = scale;
    }

    public static void SetupThinWires(this NetInfo.Node node, float scalex) {
        if (node == null || !node.m_requireWindSpeed) return; // not wire
        Vector2 scale = new Vector2(scalex, 1.0f);
        if (node.m_material) node.m_material.mainTextureScale = scale;
        if (node.m_nodeMaterial) node.m_nodeMaterial.mainTextureScale = scale;
        if (node.m_lodMaterial) node.m_lodMaterial.mainTextureScale = scale;
        if (node.m_combinedLod?.m_material) node.m_combinedLod.m_material.mainTextureScale = scale;
    }

    public static void SetupThinWires(this NetInfo netInfo, float scalex) {
        if (netInfo == null) return;
        foreach (var segment in netInfo.m_segments)
            segment?.SetupThinWires(scalex);
        foreach (var node in netInfo.m_nodes)
            node?.SetupThinWires(scalex);
        if(netInfo.GetMetaData() is NetInfoExtionsion.Net net) {
            foreach(var track in net.Tracks) {
                track?.SetupThinWires(scalex);
            }
        }
    }

    public static bool IsNearCurb(LaneData laneA, LaneData laneD, ushort nodeID) {
        float pos1 = laneA.LaneInfo.m_position;
        float pos2 = laneD.LaneInfo.m_position;
        bool nearCurb = false;
        if (pos1 != 0 && pos2 != 0) {
            laneA.Segment.GetLeftAndRightSegments(nodeID, out ushort leftSegemntID, out ushort rightSegmentID);
            bool left = leftSegemntID == laneD.SegmentID;
            bool right = rightSegmentID == laneD.SegmentID;

            bool head1 = laneA.Segment.IsInvert() == laneA.Segment.IsStartNode(nodeID);
            bool head2 = laneD.Segment.IsInvert() == laneD.Segment.IsStartNode(nodeID);

            bool neightbor = false;
            if (pos1 < 0) {
                neightbor = (head1 && left) || (!head1 && right);
            } else if (pos1 > 0) {
                neightbor = (head1 && right) || (!head1 && left);
            }

            if (neightbor) {
                if (head1 != head2) {
                    nearCurb = (pos1 < 0 && pos2 < 0) || (pos1 > 0 && pos2 > 0);
                } else {
                    nearCurb = (pos1 < 0 && pos2 > 0) || (pos1 > 0 && pos2 < 0);
                }
            }
        }
        return nearCurb;
    }
}
