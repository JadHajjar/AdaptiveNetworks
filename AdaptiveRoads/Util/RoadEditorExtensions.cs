namespace AdaptiveRoads.Util {
    using ColossalFramework.UI;
    using Epic.OnlineServices.Presence;
    using KianCommons;
    using System;
    using System.Reflection;
    using UnityEngine;

    internal static class REPropertySetExtensions {
        public static object GetTarget(this REPropertySet instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_Target");

        public static FieldInfo GetTargetField(this REPropertySet instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_TargetField") as FieldInfo;

        public static void OnPropertyChanged(this REPropertySet instance) =>
            ReflectionHelpers.InvokeMethod(instance, "OnPropertyChanged");
    }

    public static class RoadEditorMainPanelExtensions {
        public static void OnObjectModified(this RoadEditorMainPanel roadEditorMainPanel) =>
            ReflectionHelpers.InvokeMethod(roadEditorMainPanel,nameof(OnObjectModified));
        public static void Reset(this RoadEditorMainPanel roadEditorMainPanel, NetInfo info) =>
            ReflectionHelpers.InvokeMethod(roadEditorMainPanel, nameof(Reset), info);
        public static void Clear(this RoadEditorMainPanel roadEditorMainPanel) =>
            ReflectionHelpers.InvokeMethod(roadEditorMainPanel, nameof(Clear));
        public static void Initialize(this RoadEditorMainPanel roadEditorMainPanel) =>
            ReflectionHelpers.InvokeMethod(roadEditorMainPanel, nameof(Initialize));
    }


    public static class RoadEditorPanelExtensions {
        public static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(RoadEditorPanel), methodName);

        public static void CreateGenericField(this RoadEditorPanel instance,
            string groupName, FieldInfo field, object target) {
            GetMethod("CreateGenericField").Invoke(instance, new object[] { groupName, field, target });
        }

        public static void CreateField(this RoadEditorPanel instance,
            FieldInfo field, object target) {
            GetMethod("CreateField").Invoke(instance, new object[] { field, target });
        }

        public static void AddLanePropFields(this RoadEditorPanel instance) =>
            GetMethod("AddLanePropFields").Invoke(instance, null);

        public static void AddLanePropSelectField(this RoadEditorPanel instance) =>
             GetMethod("AddLanePropSelectField").Invoke(instance, null);

        public static void AddCrossImportField(this RoadEditorPanel instance) =>
            GetMethod("AddCrossImportField").Invoke(instance, null);

        public static void AddModelImportField(this RoadEditorPanel instance, bool showColorField = true) =>
            GetMethod("AddModelImportField")
            .Invoke(instance, new object[] { showColorField });

        //private void RoadEditorPanel.FitToContainer(UIComponent comp) 
        public static void FitToContainer(this RoadEditorPanel instance, UIComponent comp) =>
            GetMethod("FitToContainer")
            .Invoke(instance, new object[] { comp });

        public static void OnObjectModified(this RoadEditorPanel instance) =>
            GetMethod("OnObjectModified").Invoke(instance, null);

        public static void AddToggle(this RoadEditorPanel instance,
            RoadEditorCollapsiblePanel panel, object element, FieldInfo field, object target) {
            // private void AddToggle(RoadEditorCollapsiblePanel panel,
            //      object element, FieldInfo field, object target)
            GetMethod("AddToggle")
                .Invoke(instance, new[] { panel, element, field, target });
        }

        public static bool RequiresUserFlag(Type type) {
            return type == typeof(Building.Flags) || type == typeof(Vehicle.Flags);
        }

        public static RoadEditorCollapsiblePanel GetGroupPanel(this RoadEditorPanel instance, string name) {
            return
                (RoadEditorCollapsiblePanel)
                GetMethod("GetGroupPanel")
                .Invoke(instance, new[] { name });
        }

        public static void DestroySidePanel(this RoadEditorPanel instance) =>
            GetMethod("DestroySidePanel").Invoke(instance, null);

        public static RoadEditorPanel GetSidePanel(this RoadEditorPanel instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_SidePanel") as RoadEditorPanel;

        public static object GetTarget(this RoadEditorPanel instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_Target");
        public static void Reset(this RoadEditorPanel instance) =>
            instance.Initialize(instance.GetTarget());

    }

    internal static class RoadEditorCollapsiblePanelExtensions {
        public static RoadEditorAddButton GetAddButton(
            this RoadEditorCollapsiblePanel instance) {
            return instance.component.GetComponentInChildren<RoadEditorAddButton>();
        }
        public static FieldInfo GetField(
                this RoadEditorCollapsiblePanel instance) =>
                instance.GetAddButton()?.field;

        public static object GetTarget(this RoadEditorCollapsiblePanel instance) =>
            instance.GetAddButton()?.target;

        public static Array GetArray(this RoadEditorCollapsiblePanel instance) =>
            instance.GetField()?.GetValue(instance.GetTarget()) as Array;

        public static void SetArray(this RoadEditorCollapsiblePanel instance, Array value) =>
            instance.GetField().SetValue(instance.GetTarget(), value);
    }

    internal static class DPTHelpers {
        internal static Type DPTType =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);
        static T GetFieldValue<T>(this UICustomControl dpt, string name) {
            Assertion.Assert(dpt.GetType() == DPTType);
            return (T)ReflectionHelpers.GetFieldValue(dpt, name);
        }
        static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(DPTType, methodName);

        internal static UICustomControl GetDPTInParent(Component c) =>
            c.GetComponentInParent(DPTType) as UICustomControl;

        internal static UICustomControl GetDPTInChildren(Component c) =>
            c.GetComponentInChildren(DPTType) as UICustomControl;


        internal static UIButton GetDPTSelectButton(UICustomControl dpt) =>
            dpt.GetFieldValue<UIButton>("m_SelectButton");

        /// <summary>gets the element in array the DPT represents.</summary>
        internal static object GetDPTTargetElement(UICustomControl dpt) =>
            dpt.GetFieldValue<object>("m_TargetElement");

        /// <summary>gets the object that contains the array (m_field)</summary>
        internal static object GetDPTTargetObject(UICustomControl dpt) =>
            dpt.GetFieldValue<object>("m_TargetObject");

        /// <summary>gets the array field(m_field)</summary>
        internal static FieldInfo GetDPTField(UICustomControl dpt) =>
            dpt.GetFieldValue<object>("m_Field") as FieldInfo;

        internal static void ToggleDPTColor(UICustomControl dpt, bool selected) =>
            GetMethod("ToggleColor").Invoke(dpt, new object[] { selected });
    }

    internal static class RERefSetExtensions {
        static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(RERefSet), methodName);

        internal static void OnReferenceSelected(this RERefSet instance, PrefabInfo info) =>
            GetMethod("OnReferenceSelected")
            .Invoke(instance, new object[] { info });

        public static object GetTarget(this RERefSet instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_Target");

        public static FieldInfo GetField(this RERefSet instance) =>
            ReflectionHelpers.GetFieldValue(instance, "m_Field") as FieldInfo;
    }
}
