namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Data.NetworkExtensions;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Runtime.Serialization;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using static NetLaneProps;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class TransitionProp : IMetaData {
            #region serialization
            [Obsolete("only useful for the purpose of shallow clone and serialization", error: true)]
            public TransitionProp() { }
            public TransitionProp Clone() {
                var ret = this.ShalowClone();
                ret.Curve = ret.Curve?.ShalowClone();
                return ret;
            }

            object ICloneable.Clone() => Clone();

            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public TransitionProp(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this);
            }
            #endregion

            #region flags
            [CustomizableProperty("Node", "Flags")]
            public VanillaNodeInfoFlagsLong VanillaNodeFlags;

            [CustomizableProperty("Node Flags Extension", "Flags")]
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Transition Flags", "Flags")]
            public LaneTransitionInfoFlags TransitionFlags;

            [CustomizableProperty("Segment Flags", "Flags")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Flags Extension", "Flags")]
            [Hint("Source segment flags")]
            public SegmentInfoFlags SegmentFlags;

            [CustomizableProperty("Segment End Flags", "Flags")]
            [Hint("Source segment flags")]
            public SegmentEndInfoFlags SegmentEndFlags;

            //[CustomizableProperty("Lane", "Flags")]
            //public VanillaLaneInfoFlags VanillaLaneFlags;

            [CustomizableProperty("Lane Extension", "Flags")]
            [Hint("Source lane flags")]
            public LaneInfoFlags LaneFlags;
            #endregion

            public PropInfo m_prop;

            public TreeInfo m_tree;

            [CustomizableProperty("Position")]
            public Vector3 m_position;

            [CustomizableProperty("Angle")]
            public float m_angle;

            [CustomizableProperty("Segment Offset")]
            public float m_segmentOffset;

            [CustomizableProperty("Repeat Distance")]
            public float m_repeatDistance;

            [CustomizableProperty("Min Length")]
            public float m_minLength;

            [CustomizableProperty("Corner Angle")]
            public float m_cornerAngle;

            [CustomizableProperty("Probability")]
            public int m_probability = 100;

            [CustomizableProperty("Upgradable")]
            public bool m_upgradable;

            [NonSerialized]
            public PropInfo m_finalProp;

            [NonSerialized]
            public TreeInfo m_finalTree;

            [CustomizableProperty("Curve")]
            public Range Curve;

            [Hint("Shift due track super-elevation. " +
                "The amount of shift is proportional to sin(angle) and Catenary height which can be set in the network properties.")]
            [CustomizableProperty("Catenary")]
            public bool Catenary;

            public bool Check(
                NetNode.FlagsLong vanillaNodeFlags, NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegment.Flags vanillaSegmentFlags, NetSegmentExt.Flags segmentFlags,
                LaneTransition.Flags transitionFlags,
                NetLaneExt.Flags laneFlags,
                float laneCurve) =>
                VanillaNodeFlags.CheckFlags(vanillaNodeFlags) && NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                TransitionFlags.CheckFlags(transitionFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                LaneFlags.CheckFlags(laneFlags) &&
                Curve.CheckRange(laneCurve);

            internal CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentEndFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
                Lane = LaneFlags.UsedCustomFlags,
            };
        }
    }
}
