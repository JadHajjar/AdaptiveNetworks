namespace AdaptiveRoads.UI.Tool {
    extern alias UnifedUILib;
    using System;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.Tool;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using ColossalFramework.UI;
    using UnifedUILib::UnifiedUI.Helpers;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using System.Collections.Generic;
    using ColossalFramework;

    internal class ARTool : KianToolBase<ARTool> {
        private ushort[] selectedSegmentIDs_;
        public ushort[] SelectedSegmentIDs {
            get => selectedSegmentIDs_ ?? new ushort[0];
            set => selectedSegmentIDs_ = value;
        }
        public ushort SelectedSegmentID;
        public ushort SelectedNodeID;
        public bool SelectedStartNode => SelectedSegmentID.ToSegment().IsStartNode(SelectedNodeID);
        public ushort[] GetHoveredSegmentIDs() => Util.GetSimilarSegmentsBetweenJunctions(HoveredSegmentID).ToArray();


        public static bool NodeMode => Helpers.ControlIsPressed;
        public static bool SegmentEndMode => Helpers.AltIsPressed;

        public static bool SegmentMode => !NodeMode && !SegmentEndMode;

        public static bool MultiSelectMode => Helpers.ShiftIsPressed;

        UIComponent button_;
        FlagsPanel flagsPanel_;

        void OpenPanel() {
            ClosePanel();
            flagsPanel_ = FlagsPanel.Open(SelectedNodeID, SelectedSegmentID, SelectedSegmentIDs);
        }

        void ClosePanel() {
            flagsPanel_?.Close();
            flagsPanel_ = null;
        }

        protected override void OnPrimaryMouseClicked() {
            LogCalled();
            if (!CanCustomiseHover())
                return;
            if (SegmentMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = 0;
                if(MultiSelectMode) {
                    SelectedSegmentIDs = GetHoveredSegmentIDs();
                } else {
                    SelectedSegmentIDs = null;
                }
            } else if (NodeMode) {
                SelectedNodeID = HoveredNodeID;
                SelectedSegmentID = 0;
                SelectedSegmentIDs = null;
            } else if (SegmentEndMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = HoveredNodeID;
                SelectedSegmentIDs = null;
            }
            OpenPanel();
        }

        protected override void OnSecondaryMouseClicked() {
            LogCalled();
            if(SelectedSegmentID != 0 || SelectedNodeID != 0) {
                SelectedSegmentIDs = null;
                SelectedSegmentID = 0;
                SelectedNodeID = 0;
                ClosePanel();
            } else {
                enabled = false;
            }
        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();

            if (CanCustomiseHover()) {
                ToolCursor = ToolsModifierControl.toolController.Tools.OfType<NetTool>().FirstOrDefault()?.m_upgradeCursor;
            } else {
                ToolCursor = null;
            }

            if (HoverValid) {
                var hints = new List<string>();
                var usedCustomFlags = GetUsedFlagsSegment(HoveredSegmentID);
                var segmentUserData = HoveredSegmentID.ToSegment().Info.GetMetaData()?.UserDataNamesSet?.Segment;
                bool hasSegmentUserData = segmentUserData != null && !segmentUserData.IsEmpty();
                if (usedCustomFlags.Segment != 0 || usedCustomFlags.Lane != 0 || hasSegmentUserData) {
                    hints.Add("Click => modify custom segment/lane flags");
                    hints.Add("Shift + Click => Select all segments between two junctions.");
                }
                if(GetUsedFlagsNode(HoveredNodeID) != 0)
                    hints.Add("CTRL + Click => modify node flags");
                if(GetUsedFlagsSegmentEnd(segmentID: HoveredSegmentID, nodeID: HoveredNodeID) != 0)
                    hints.Add("ALT + Click => modify segmentEnd flags");
                else if(HoveredSegmentID.ToSegment().Info.HasTitlableTracks())
                    hints.Add("ALT + Click => modify segmentEnd angle");
                if(hints.Count == 0)
                    hints.Add("This road has no custom AN flags to modify.");
                ShowToolInfo(true, hints.JoinLines(), HitPos);
            } else {
                ShowToolInfo2("Page Down => underground\n" + "Page Up => overground");
            }
        }

        public static NetNodeExt.Flags GetUsedFlagsNode(ushort nodeID) {
            NetNodeExt.Flags ret = nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            foreach(ushort segmentID in nodeID.ToNode().IterateSegments()) {
                ret |= segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            }
            return ret;
        }

        public static CustomFlags GetUsedFlagsSegment(ushort segmentID) {
            CustomFlags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;

            // Considering that nodes are segment ends we don't need to take their flags into account.
            //ushort startNodeID = segmentID.ToSegment().m_startNode;
            //ushort endNodeID = segmentID.ToSegment().m_endNode;
            //ret |= startNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;
            //ret |= endNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;
            return ret;
        }

        public static NetSegmentEnd.Flags GetUsedFlagsSegmentEnd(ushort segmentID, ushort nodeID) {
            NetSegmentEnd.Flags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;

            // Considering that nodes are segment ends we don't need to take their flags into account.
            // ret |= nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;
            return ret;
        }

        public bool CanCustomiseHover() {
            if (NodeMode) {
                return GetUsedFlagsNode(HoveredNodeID) != 0;
            } else if (SegmentMode) {
                var usedCustomFlags = GetUsedFlagsSegment(HoveredSegmentID);
                var userData = HoveredSegmentID.ToSegment().Info.GetMetaData()?.UserDataNamesSet?.Segment;
                bool hasUserData = userData != null && !userData.IsEmpty();
                return usedCustomFlags.Segment != 0 || usedCustomFlags.Lane != 0 || hasUserData;
            } else if (SegmentEndMode) {
                bool hasCustomFlags = GetUsedFlagsSegmentEnd(segmentID:HoveredSegmentID, nodeID:HoveredNodeID) != 0;
                bool canTilt = HoveredSegmentID.ToSegment().Info.GetMetaData()?.HasTitlableTracks ?? false;
                return hasCustomFlags || canTilt;
            }

            throw new Exception("Unreachable code");
        }

        public static void LogModes() {
            Log.Info($"SegmentMode={SegmentMode} NodeMode={NodeMode} SegmentEndMode={SegmentEndMode}");
        }

        public void RenderCornerOverlay(RenderManager.CameraInfo cameraInfo) {
            if(HoverValid) {
                ref var segEnd = ref NetworkExtensionManager.Instance.GetSegmentEnd(HoveredSegmentID, HoveredNodeID);
                RenderUtil.DrawOverlayCircle(cameraInfo, Color.green, segEnd.Corner.Right.Position, 2);
                RenderUtil.DrawOverlayCircle(cameraInfo, Color.yellow, segEnd.Corner.Left.Position, 2);
            }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);
            // uncomment to test
            //RenderCornerOverlay(cameraInfo);
            //RenderLaneOverlay(cameraInfo);
            //return;


            if(SelectedSegmentID != 0 && SelectedNodeID != 0)
                HighlightSegmentEnd(cameraInfo, SelectedSegmentID, SelectedNodeID, Color.white);
            else if(SelectedNodeID != 0)
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, SelectedNodeID, true);
            else if(SelectedSegmentID != 0) {
                RenderUtil.RenderSegmnetOverlay(cameraInfo, SelectedSegmentID, Color.white, true);
                foreach(var segmentID in SelectedSegmentIDs)
                    RenderUtil.RenderSegmnetOverlay(cameraInfo, segmentID, Color.white, true);
            }


            if(flagsPanel_ && flagsPanel_.HighlighLaneID != 0) {
                var laneData = new LaneData(flagsPanel_.HighlighLaneID);
                RenderUtil.RenderLaneOverlay(cameraInfo, laneData, Color.yellow, false);
                foreach(var laneData2 in Util.GetSimilarLanes(laneData, selectedSegmentIDs_)) {
                    RenderUtil.RenderLaneOverlay(cameraInfo, laneData, Color.yellow, false);
                }
            }

            if (!HoverValid)
                return;

            Color color;
            if (Input.GetMouseButton(0))
                color = GetToolColor(true, false);
            else if (CanCustomiseHover())
                color = GetToolColor(false, false);
            else
                color = GetToolColor(false, true);

            if (SegmentMode) {
                RenderUtil.RenderSegmnetOverlay(cameraInfo, HoveredSegmentID, color, true);
                if(MultiSelectMode) {
                    foreach(var segmentID in GetHoveredSegmentIDs())
                        RenderUtil.RenderSegmnetOverlay(cameraInfo, segmentID, color, true);
                }
            } else if (NodeMode) {
                RenderUtil.DrawNodeCircle(cameraInfo, color, HoveredNodeID, true);
            } else if (SegmentEndMode) {
                HighlightSegmentEnd(cameraInfo, HoveredSegmentID, HoveredNodeID, color);
            }
        }

        static void HighlightSegmentEnd(RenderManager.CameraInfo cameraInfo, ushort segmentID, ushort nodeID, Color color, bool alpha = false) {
            RenderUtil.DrawCutSegmentEnd(
                cameraInfo: cameraInfo,
                segmentId: segmentID,
                cut: 0.5f,
                bStartNode: segmentID.ToSegment().IsStartNode(nodeID),
                color: color,
                alpha: alpha);
        }

        protected override void Awake() {
            try {
                base.Awake();
                string iconPath = UUIHelpers.GetFullPath<LifeCycle.UserMod>("uui_ar.png");
                button_ = UUIHelpers.RegisterToolButton(
                    name: "AdaptiveNetworks",
                    groupName: null, // default group
                    tooltip: "Adaptive Networks",
                    tool: this,
                    icon: UUIHelpers.LoadTexture(iconPath),
                    hotkeys: new UUIHotKeys { ActivationKey = ModSettings.Hotkey });
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            button_?.Destroy();
            flagsPanel_?.Close();
            SetAllDeclaredFieldsToNull(this);
        }

        protected override void OnDisable() {
            base.OnDisable();
            ClosePanel();
            SelectedNodeID = 0;
            SelectedSegmentID = 0;
        }
    }
}
