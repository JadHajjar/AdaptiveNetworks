namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    public static class AdvanedFlagsExtensions {
        public static bool IsFlagSet(this NetLaneExt.Flags flags, NetLaneExt.Flags flag) =>
            (flags & flag) != 0;
        public static bool IsFlagSet(this NetSegmentEnd.Flags flags, NetSegmentEnd.Flags flag) =>
            (flags & flag) != 0;
        public static bool IsFlagSet(this NetSegmentExt.Flags flags, NetSegmentExt.Flags flag) =>
            (flags & flag) != 0;
        public static bool IsFlagSet(this NetNodeExt.Flags flags, NetNodeExt.Flags flag) =>
            (flags & flag) != 0;
        public static bool IsFlagSet(this LaneTransition.Flags flags, LaneTransition.Flags flag) =>
            (flags & flag) != 0;

        public static bool CheckFlags(this NetLaneExt.Flags value, NetLaneExt.Flags required, NetLaneExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentEnd.Flags value, NetSegmentEnd.Flags required, NetSegmentEnd.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentExt.Flags value, NetSegmentExt.Flags required, NetSegmentExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetNodeExt.Flags value, NetNodeExt.Flags required, NetNodeExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this LaneTransition.Flags value, LaneTransition.Flags required, LaneTransition.Flags forbidden) =>
            (value & (required | forbidden)) == required;

        public static NetLaneExt.Flags SetMaskedFlags(this NetLaneExt.Flags flags, NetLaneExt.Flags value, NetLaneExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetSegmentEnd.Flags SetMaskedFlags(this NetSegmentEnd.Flags flags, NetSegmentEnd.Flags value, NetSegmentEnd.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetSegmentExt.Flags SetMaskedFlags(this NetSegmentExt.Flags flags, NetSegmentExt.Flags value, NetSegmentExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetNodeExt.Flags SetMaskedFlags(this NetNodeExt.Flags flags, NetNodeExt.Flags value, NetNodeExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static LaneTransition.Flags SetMaskedFlags(this LaneTransition.Flags flags, LaneTransition.Flags value, LaneTransition.Flags mask) =>
            (flags & ~mask) | (value & mask);
    }
}

