namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using System;
    using System.Linq;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), "Instantiate")]
    public static class Instantiate {
        public static void Prefix(NetInfo template) {
            Log.Debug($"Instantiate.Prefix({template}) was called." /*+ Environment.StackTrace*/,false);
            //LogExtended(template);
        }
        public static void LogExtended(NetInfo info) {
            Log.Debug($"LogExtended({info}) was called",false);
            Log.Info("info.GetMetaData() ->" + info.GetMetaData().ToSTR());
            //Log.Info("shift ->" + (info.GetMetaData()?.Shift).ToSTR());
            Log.Info("any node is IInfoExtended ->" + info.m_nodes.Any(item => item is IInfoExtended));
            Log.Info("any segment is IInfoExtended ->" + info.m_segments.Any(item => item is IInfoExtended));
            Log.Info("any prop is IInfoExtended ->" + info.m_lanes
                .Any(item => item.m_laneProps.m_props
                .Any(item2=>item2 is IInfoExtended)));
        }

        /// <summary>
        /// clone list and metadata that
        /// AssetEditorRoadUtils has copied using UnityEngine.Instantiate<NetInfo>(template)
        /// we clone the template instead of the return value.
        /// at this point m_editPrefabInfo is NOT set yet.
        /// </summary>
        public static void Postfix(NetInfo __result, NetInfo template) {
            try {
                Log.Debug($"Instantiate.PostFix({template})->{__result} was called");
                if (UI.ModSettings.ARMode && template.IsAdaptive()) {
                        Log.Debug("copy metadata");
                        AssetData.NetInfoMetaData.CopyMetadata(template, __result);
                } else {
                    Log.Debug("skip copying metadata.");
                }
            } catch(Exception ex) {
                ex.Log();
            }
        }
    }
}


