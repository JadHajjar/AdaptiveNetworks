namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using KianCommons;
    using KianCommons.Serialization;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using UnityEngine;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;
    using static AdaptiveRoads.UI.ModSettings;
    using static KianCommons.ReflectionHelpers;

    public static partial class NetInfoExtionsion {
        [Serializable]
        [Optional(AR_MODE)]
        public class Net : ICloneable, ISerializable {
            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Net() { }
            public Net Clone() {
                var ret = this.ShalowClone();
                ret.ConnectGroups = ret.ConnectGroups?.ToArray();
                ret.ConnectGroupsHash = ret.ConnectGroupsHash?.ToArray();
                ret.NodeConnectGroupsHash = ret.NodeConnectGroupsHash?.ToArray();
                ret.QuayRoadsProfile = QuayRoadsProfile?.ToArray();
                ret.CustomFlagNames = ret.CustomFlagNames?.ToDictionary(entry => entry.Key, entry => entry.Value);
                ret.CustomLaneFlagNames0 = ret.CustomLaneFlagNames0?.ToDictionary(entry => entry.Key, entry => entry.Value);
                ret.CustomLaneFlagNames = ret.CustomLaneFlagNames
                    ?.Select(item => item?.ToDictionary(entry => entry.Key, entry => entry.Value))
                    ?.ToArray();
                for(int i = 0; i < ret.Tracks.Length; ++i) {
                    ret.Tracks[i] = ret.Tracks[i].Clone();
                }

                return ret;
            }

            object ICloneable.Clone() => Clone();
            public Net(NetInfo template) {
                PavementWidthRight = template.m_pavementWidth;
                UsedCustomFlags = GatherUsedCustomFlags(template);
                Template = template;
            }

            public void ReleaseModels() {
                foreach(var track in Tracks) {
                    track.ReleaseModel();
                }
            }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                FillCustomLaneFlagNames();
                SerializationUtil.GetObjectFields(info, this);
            }

            // deserialization
            public Net(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion

            [NonSerialized]
            public NetInfo Template;

            public string[] ConnectGroups;

            [NonSerialized]
            public int[] NodeConnectGroupsHash;


            [NonSerialized]
            public int[] ConnectGroupsHash;

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Pavement Width Right", "Properties")]
            public float PavementWidthRight;

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Shift", "Properties")]
            [Hint("shifts road right-wards (when going from tail to head)")]
            public float Shift = 0;

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Catenary Height", "Properties")]
            [Hint("Catenaries and wires are shifted based on super elevation and height")]
            public float CatenaryHeight = 2.5f;

            [AfterField(nameof(NetInfo.m_minCornerOffset))]
            [CustomizableProperty("Sharp Corners", "Properties")]
            [Hint("only works when corner is 90deg")]
            public bool SharpCorners;

            [AfterField(nameof(NetInfo.m_minCornerOffset))]
            [CustomizableProperty("Parking Angle °", "Properties")]
            public float ParkingAngleDegrees = 0;

            /// <summary>
            /// 1/sin(ParkingAngleDegrees)
            /// </summary>
            [NonSerialized]
            public float OneOverSinOfParkingAngle = 1;

            public Data.QuayRoads.ProfileSection[] QuayRoadsProfile = null;

            #region Custom Flags
            [NonSerialized]
            public CustomFlags UsedCustomFlags;

            public Dictionary<Enum, string> CustomFlagNames = new Dictionary<Enum, string>();

            [NonSerialized]
            public Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>> CustomLaneFlagNames0;

            public Dictionary<NetLaneExt.Flags, string>[] CustomLaneFlagNames;

            [CustomizableProperty("Tracks")]
            [AfterField("m_nodes")]
            public Track[] Tracks = new Track[0];

            [NonSerialized]
            public ulong TrackLanes;

            [NonSerialized]
            public int TrackLaneCount;

            public bool HasTrackLane(int laneIndex) => ((1ul << laneIndex) & TrackLanes) != 0;

            static CustomFlags GatherUsedCustomFlags(NetInfo info) {
                var ret = CustomFlags.None;
                foreach(var item in info.m_nodes) {
                    if(item.GetMetaData() is Node metaData)
                        ret |= metaData.UsedCustomFlags;
                }

                foreach(var item in info.m_segments) {
                    if(item.GetMetaData() is Segment metaData)
                        ret |= metaData.UsedCustomFlags;
                }

                foreach(var lane in info.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    if(props.IsNullorEmpty()) continue;
                    foreach(var item in props) {
                        if(item.GetMetaData() is LaneProp metaData)
                            ret |= metaData.UsedCustomFlags;
                    }
                }

                foreach(var track in info.GetMetaData()?.Tracks ?? Enumerable.Empty<Track>()) {
                    ret |= track.UsedCustomFlags;
                }

                return ret;
            }

            private void FillCustomLaneFlagNames() {
                try {
                    CustomLaneFlagNames = null;
                    if(CustomLaneFlagNames.IsNullorEmpty()) return;
                    CustomLaneFlagNames = new Dictionary<NetLaneExt.Flags, string>[Template.m_lanes.Length];
                    for(int laneIndex = 0; laneIndex < CustomLaneFlagNames.Length; ++laneIndex) {
                        var lane = Template.m_lanes[laneIndex];
                        if(CustomLaneFlagNames0.TryGetValue(lane, out var dict)) {
                            CustomLaneFlagNames[laneIndex] = dict;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }
            }

            public string GetCustomLaneFlagName(NetLaneExt.Flags flag, int laneIndex) {
                try {
                    if(CustomLaneFlagNames0 is not null) {
                        // edit prefab
                        var lane = Template.m_lanes[laneIndex];
                        if(CustomLaneFlagNames0.TryGetValue(lane, out var dict) &&
                            dict.TryGetValue(flag, out string name)) {
                            return name;
                        }

                    } else if(CustomLaneFlagNames is not null) {
                        // normal
                        Assertion.InRange(CustomLaneFlagNames, laneIndex);
                        var dict = CustomLaneFlagNames[laneIndex];
                        if(dict != null && dict.TryGetValue(flag, out string name)) {
                            return name;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }

                return null;
            }

            public static string GetCustomFlagName(Enum flag, object target) {
                try {
                    if(target is NetLaneProps.Prop prop && flag is NetLaneExt.Flags laneFlag) {
                        var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                        return netInfo?.GetMetaData()?.GetCustomLaneFlagName(laneFlag, laneIndex);
                    } else {
                        var netInfo =
                            (target as NetInfo.Node)?.GetParent(out _) ??
                            (target as NetInfo.Segment)?.GetParent(out _) ??
                            (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        var dict = netInfo?.GetMetaData()?.CustomFlagNames;
                        if(dict != null && dict.TryGetValue(flag, out string name)) {
                            return name;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }
                return null;
            }

            public static event Action OnCustomFlagRenamed;
            public void RenameCustomFlag(Enum flag, string name) {
                try {
                    CustomFlagNames ??= new Dictionary<Enum, string>();
                    if(name.IsNullOrWhiteSpace() || name == flag.ToString())
                        CustomFlagNames.Remove(flag);
                    else
                        CustomFlagNames[flag] = name;
                    OnCustomFlagRenamed?.Invoke();
                } catch(Exception ex) { ex.Log(); }
            }

            public void RenameCustomFlag(int laneIndex, NetLaneExt.Flags flag, string name) {
                try {
                    Assertion.NotNull(Template, "Template");
                    var lane = Template.m_lanes[laneIndex];
                    Dictionary<NetLaneExt.Flags, string> dict = null;

                    CustomLaneFlagNames0 ??= new Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>>();
                    if(!CustomLaneFlagNames0.TryGetValue(lane, out dict)) {
                        dict = CustomLaneFlagNames0[lane] = new Dictionary<NetLaneExt.Flags, string>();
                    }

                    if(name.IsNullOrWhiteSpace() || name == flag.ToString())
                        dict.Remove(flag);
                    else
                        dict[flag] = name;

                    OnCustomFlagRenamed?.Invoke();
                } catch(Exception ex) { ex.Log(); }
            }

            public static void RenameCustomFlag(Enum flag, object target, string name) {
                try {
                    if(target is NetLaneProps.Prop prop && flag is NetLaneExt.Flags laneFlag) {
                        var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                        netInfo.GetMetaData().RenameCustomFlag(laneIndex: laneIndex, flag: (NetLaneExt.Flags)flag, name: name);
                    } else {
                        var netInfo =
                            (target as NetInfo.Node)?.GetParent(out _) ??
                            (target as NetInfo.Segment)?.GetParent(out _) ??
                            (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        netInfo.GetMetaData().RenameCustomFlag(flag: flag, name: name);
                    }
                } catch(Exception ex) { ex.Log(); }
            }

            /// <summary>
            /// Gets the used lane flags for the input lane (not all the lanes of the NetInfo)
            /// </summary>
            public static NetLaneExt.Flags GetUsedCustomFlagsLane(NetInfo.Lane laneInfo) {
                NetLaneExt.Flags mask = 0;
                var props = (laneInfo.m_laneProps?.m_props).EmptyIfNull();
                foreach(var prop in props) {
                    var metadata = prop.GetMetaData();
                    if(metadata != null)
                        mask |= (metadata.LaneFlags.Required | metadata.LaneFlags.Forbidden);
                }
                return mask & NetLaneExt.Flags.CustomsMask;
            }
            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="netInfo"></param>
            public void Recalculate(NetInfo netInfo) {
                try {
                    Assertion.NotNull(netInfo, "netInfo");
                    Template = netInfo;
                    RecalculateTracks(netInfo);
                    RefreshLevelOfDetail(netInfo);
                    UsedCustomFlags = GatherUsedCustomFlags(netInfo);
                    RecalculateParkingAngle();
                    RecalculateConnectGroups(netInfo);
                } catch(Exception ex) { ex.Log(); }
            }

            public void RefreshLevelOfDetail(NetInfo netInfo) {
                if(Tracks != null) {
                    Log.Called();
                    var max = Mathf.Max(netInfo.m_halfWidth * 50f, (netInfo.m_maxHeight - netInfo.m_minHeight) * 80f);
                    // get lod render instance from already existing nodes/segments. if not found then calculate.
                    float lodRenderDistance =
                        netInfo.m_segments?.FirstOrDefault()?.m_lodRenderDistance ??
                        netInfo.m_nodes?.FirstOrDefault()?.m_lodRenderDistance ??
                        Mathf.Clamp(100f + RenderManager.LevelOfDetailFactor * max, 100f, 1000f);

                    bool lodMissing = false;
                    TrackLanes = 0;
                    for(int i = 0; i < Tracks.Length; i++) {
                        var track = Tracks[i];
                        bool hasLod = track.m_mesh;
                        if(hasLod) {
                            track.m_lodRenderDistance = lodRenderDistance;
                        } else {
                            track.m_lodRenderDistance = 100000f;
                            lodMissing = true;
                        }
                        netInfo.m_netLayers |= 1 << track.m_layer;
                        this.TrackLanes |= track.LaneIndeces;
                    }
                    if(lodMissing) {
                        CODebugBase<LogChannel>.Warn(LogChannel.Core, "LOD missing: " + netInfo.gameObject.name, netInfo.gameObject);
                    }
                }
            }
            void RecalculateTracks(NetInfo netInfo) {
                if(Tracks != null) {
                    // has color been already assigned in NetInfo.InitializePrefab() ?
                    bool hasColor = netInfo.m_segments?.Any(item => item.m_material) ?? false;
                    hasColor = hasColor || (netInfo.m_nodes?.Any(item => item.m_material) ?? false);
                    TrackLanes = 0;
                    for(int i = 0; i < Tracks.Length; i++) {
                        var track = Tracks[i];
                        track.Recalculate(netInfo);
                        track.CachedArrayIndex = i;
                        bool hasLod = track.m_mesh;
                        if(!hasColor && track.m_material != null) {
                            netInfo.m_color = track.m_material.color;
                            hasColor = true;
                        }
                        netInfo.m_netLayers |= 1 << track.m_layer;
                        this.TrackLanes |= track.LaneIndeces;
                    }

                    TrackLaneCount = EnumBitMaskExtensions.CountOnes(TrackLanes);
                    netInfo.m_requireDirectRenderers |= TrackLaneCount > 0;
                }
            }

            // NetInfo
            // Token: 0x06004ED3 RID: 20179 RVA: 0x00246F24 File Offset: 0x00245324
            [NonSerialized]
            public static NetInfo.Segment tempSegmentInfo_ = new NetInfo.Segment();
            public static NetInfo.Segment TempSegmentInfo(Track trackInfo) {
                tempSegmentInfo_.m_lodMesh = trackInfo.m_lodMesh;
                tempSegmentInfo_.m_lodMaterial = trackInfo.m_lodMaterial;
                tempSegmentInfo_.m_combinedLod = trackInfo.m_combinedLod;
                tempSegmentInfo_.m_preserveUVs = trackInfo.m_preserveUVs;
                tempSegmentInfo_.m_generateTangents = trackInfo.m_generateTangents;
                tempSegmentInfo_.m_layer = trackInfo.m_layer;
                return tempSegmentInfo_;
            }

            public void InitMeshData(Track trackInfo, Rect atlasRect, Texture2D rgbAtlas, Texture2D xysAtlas, Texture2D aprAtlas) {
                if(Log.VERBOSE) Log.Debug("InitMeshData() for trackInfo called");
                // work around private fields/methods.
                var tempSegment = TempSegmentInfo(trackInfo);
                Template.InitMeshData(TempSegmentInfo(trackInfo), atlasRect, rgbAtlas, xysAtlas, aprAtlas);
                trackInfo.m_combinedLod = tempSegment.m_combinedLod;
                trackInfo.UpdateScale(Template);
            }

            void RecalculateParkingAngle() {
                float sin = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * ParkingAngleDegrees));
                if(sin >= Mathf.Sin(30))
                    OneOverSinOfParkingAngle = 1 / sin;
                else
                    OneOverSinOfParkingAngle = 1;
            }

            void RecalculateConnectGroups(NetInfo netInfo) {
                LogCalled();
                ConnectGroupsHash = ConnectGroups?.Select(item => item.GetHashCode()).ToArray();
                if(ConnectGroupsHash.IsNullorEmpty()) ConnectGroupsHash = null;

                foreach(var node in netInfo.m_nodes)
                    node.GetMetaData()?.Update();

                NodeConnectGroupsHash = GetNodeConnectGroupsHash(netInfo).ToArray();
                if(NodeConnectGroupsHash.IsNullorEmpty()) NodeConnectGroupsHash = null;

                var itemSource = ItemSource.GetOrCreate(typeof(NetInfo.ConnectGroup));
                foreach(var connectGroup in GetAllConnectGroups(netInfo))
                    itemSource.Add(connectGroup);
            }

            IEnumerable<int> GetNodeConnectGroupsHash(NetInfo netInfo) {
                foreach(var node in netInfo.m_nodes) {
                    var hashes = node.GetMetaData()?.ConnectGroupsHash;
                    if(hashes == null) continue;
                    foreach(int hash in hashes)
                        yield return hash;
                }
            }

            IEnumerable<string> GetAllConnectGroups(NetInfo netInfo) {
                if(ConnectGroups != null) {
                    foreach(var cg in ConnectGroups)
                        yield return cg;
                }

                foreach(var node in netInfo.m_nodes) {
                    var connectGroups = node.GetMetaData()?.ConnectGroups;
                    if(connectGroups == null) continue;
                    foreach(var cg in connectGroups)
                        yield return cg;
                }
            }
        }
    }
    public class NetMetadataContainer : MonoBehaviour {
        public Net Metadata;
        void OnDestroy() => Metadata = null;

        public static NetMetadataContainer GetContainer(NetInfo info) =>
            info?.gameObject.GetComponent<NetMetadataContainer>();
        public static Net GetMetadata(NetInfo info) =>
            GetContainer(info)?.Metadata;

        public static NetMetadataContainer GetOrCreateContainer(NetInfo info) {
            Assertion.Assert(info, "info");
            return GetContainer(info) ??
                info.gameObject.AddComponent<NetMetadataContainer>();
        }
        public static Net GetOrCreateMetadata(NetInfo info) {
            var container = GetOrCreateContainer(info);
            return container.Metadata ??= new Net(info);
        }

        public static void SetMetadata(NetInfo info, Net value) {
            Assertion.Assert(info, "info");
            GetOrCreateContainer(info).Metadata = value;
        }

        public static void RemoveContainer(NetInfo info) {
            Assertion.Assert(info, "info");
            DestroyImmediate(GetContainer(info));
        }
    }

}