namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using AdaptiveRoads.Data.NetworkExtensions;
    using System.Reflection;
    using System.Runtime.Serialization;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
            public override string ToString() => $"[{Lower}:{Upper})";
        }

        [Serializable]
        [FlagPair]
        public struct VanillaSegmentInfoFlags {
            [BitMask]
            public NetSegment.Flags Required, Forbidden;
            public bool CheckFlags(NetSegment.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Obsolete("use VanillaNodeInfoFlagsLong instead")]
        //[FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlags {
            [BitMask]
            public NetNode.Flags Required, Forbidden;
            //public bool CheckFlags(NetNode.Flags flags) => flags.CheckFlags(Required, Forbidden);

            public static explicit operator VanillaNodeInfoFlagsLong(VanillaNodeInfoFlags flags) {
                return new VanillaNodeInfoFlagsLong {
                    Required = (NetNode.FlagsLong)flags.Required,
                    Forbidden = (NetNode.FlagsLong)flags.Forbidden,
                };
            }

            // legacy deserialization.
            public static void SetObjectFields(SerializationInfo info, object target) {
                try {
                    foreach (SerializationEntry item in info) {
                        if (item.Value is VanillaNodeInfoFlags flags) {
                            VanillaNodeInfoFlagsLong val = (VanillaNodeInfoFlagsLong)flags;
                            FieldInfo field = target.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                            if (field != null) {
                                field.SetValue(target, val);
                            }
                        }
                    }
                } catch (Exception ex) { ex.Log(); }
            }
        }

        [FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlagsLong {
            [BitMask]
            public NetNode.FlagsLong Required, Forbidden;
            public bool CheckFlags(NetNode.FlagsLong flags) => flags.CheckFlags(Required, Forbidden);
        }


        [Serializable]
        [FlagPair]
        public struct VanillaLaneInfoFlags {
            [BitMask]
            public NetLane.Flags Required, Forbidden;
            public bool CheckFlags(NetLane.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetSegment.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetSegmentFlags))]
        [Serializable]
        public struct SegmentInfoFlags {
            [BitMask]
            public NetSegmentExt.Flags Required, Forbidden;
            internal NetSegmentExt.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentExt.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        [Hint("segment specific node flags")]
        public struct SegmentEndInfoFlags {
            [BitMask]
            public NetSegmentEnd.Flags Required, Forbidden;
            internal NetSegmentEnd.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentEnd.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetNode.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetNode.Flags2))]
        [FlagPair(MergeWithEnum = typeof(NetNode.FlagsLong))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags2))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlagsLong))]
        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            internal NetNodeExt.Flags UsedCustomFlags => (Required | Forbidden) & NetNodeExt.Flags.CustomsMask;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneInfoFlags {
            [BitMask]
            public NetLaneExt.Flags Required, Forbidden;
            internal NetLaneExt.Flags UsedCustomFlags => (Required | Forbidden) & NetLaneExt.Flags.CustomsMask;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneTransitionInfoFlags {
            [BitMask]
            public LaneTransition.Flags Required, Forbidden;
            public bool CheckFlags(LaneTransition.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }
    }
}


