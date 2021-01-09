namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.Templates;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using static Util.DPTHelpers;
    using static AdaptiveRoads.Util.RoadEditorUtils;

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnButtonClick")]
    internal static class RoadEditorCollapsiblePanelPatch {
        [HarmonyPrefix]
        static bool OnButtonClickPrefix(UIComponent component) {
            if (!HelpersExtensions.ControlIsPressed)
                return true;
            OnButtonControlClick(component);
            return false;
        }

        static void OnButtonControlClick(UIComponent component) {
            try {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                Array array = groupPanel.GetArray();
                var target = groupPanel.GetTarget();
                if (groupPanel.GetArray() is NetLaneProps.Prop[] m_props) {
                    bool hasItems = m_props.Length > 0;
                    bool clipBoardHasData = ClipBoard.HasData<NetLaneProps.Prop>();
                    if (hasItems || clipBoardHasData) {
                        var panel = MiniPanel.Display();
                        if (hasItems) {
                            panel.AddButton("Copy all props", null,
                            () => ClipBoard.SetData(m_props));
                            panel.AddButton("Clear all props", null,
                                () => ClearAll(groupPanel));
                            panel.AddButton("Save Tamplate", null, () => {
                                SaveTemplatePanel.Display(m_props);
                            });
                        }
                        if (clipBoardHasData) {
                            panel.AddButton("Paste all props", null,
                                () => PasteAll(groupPanel));
                        }
                        panel.AddButton(
                            "Displace all",
                            null,
                            () => DisplaceAll(m_props));
                    }
                } else if (
                    array is NetInfo.Lane[] m_lanes
                    && m_lanes.Any(_lane => _lane.HasProps())
                    && target == NetInfoExtionsion.EditedNetInfo) {
                    var panel = MiniPanel.Display();
                    panel.AddButton(
                        "Copy props to other elevation",
                        "appends props to other elevations",
                        () => PropHelpers.CopyPropsToOtherElevations(clear: false));
                    panel.AddButton(
                        "replace props to other elevation",
                        "clears props from other elevations before copying.",
                        () => PropHelpers.CopyPropsToOtherElevations(clear: true));
                    panel.AddButton(
                        "Displace all",
                        null,
                        () => RoadEditorUtils.DisplaceAllProps(m_lanes));
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void ClearAll(RoadEditorCollapsiblePanel instance) {
            try {
                var roadEditor = instance.component.GetComponentInParent<RoadEditorPanel>();
                var sidePanel = roadEditor.GetSidePanel();
                if (sidePanel != null && sidePanel.GetTarget() is NetLaneProps.Prop) {
                    Log.Debug("Destroying prop side panel");
                    roadEditor.DestroySidePanel();
                }

                var toggles = instance.m_Panel.GetComponentsInChildren(DPTType);
                foreach (UICustomControl toggle in toggles) {
                    toggle.component.parent.RemoveUIComponent(toggle.component);
                    UnityEngine.Object.Destroy(toggle.gameObject);
                }


                instance.SetArray(new NetLaneProps.Prop[0]);
                roadEditor.OnObjectModified();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void PasteAll(RoadEditorCollapsiblePanel groupPanel) {
            Log.Debug("PasteAll called");
            NetLaneProps.Prop[] props = ClipBoard.GetDataArray() as NetLaneProps.Prop[];
            AddProps(groupPanel, props);
        }

    }
}
