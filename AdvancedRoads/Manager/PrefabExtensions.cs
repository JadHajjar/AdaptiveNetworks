using KianCommons;
using System;
using PrefabIndeces;

namespace AdvancedRoads.Manager {
    [Serializable]
    public class NetInfoExt {
        [Serializable]
        public class SegmentInfoFlags {
            public NetSegmentExt.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class SegmentEndInfoFlags {
            public NetSegmentEnd.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class LaneInfoFlags {
            public NetLaneExt.Flags Required, Forbidden;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class SegmentInfoExt {
            public SegmentInfoFlags Flags;
            public bool CheckFlags(NetSegmentExt.Flags flags) => Flags.CheckFlags(flags);

            public SegmentInfoExt(NetInfo.Segment template) { }

            public static SegmentInfoExt Get(NetInfoExtension.Segment IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].SegmentInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class Node {
            public NodeInfoFlags NodeFlags;
            public SegmentEndInfoFlags SegmentEndFlags;
            public bool CheckFlags(NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags);

            public Node(NetInfo.Node template) { }

            public static Node Get(NetInfoExtension.Node IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].NodeInfoExts[IndexExt.Index];
            }

        }

        [Serializable]
        public class LaneInfoExt {
            public LaneInfoFlags LaneFlags;

            [NonSerialized]
            public PropInfoExt[] PropInfoExts;

            public LaneInfoExt(NetInfo.Lane template) {
                PropInfoExts = new PropInfoExt[template.m_laneProps.m_props.Length];
                for (int i = 0; i < PropInfoExts.Length; ++i) {
                    PropInfoExts[i] = new PropInfoExt(template.m_laneProps.m_props[i]);
                }
            }

            public static LaneInfoExt Get(NetInfoExtension.Lane IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].LaneInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class PropInfoExt {
            public LaneInfoFlags LaneFlags;
            public SegmentInfoFlags SegmentFlags;
            public SegmentEndInfoFlags SegmentStartFlags, SegmentEndFlags;
            public NodeInfoFlags StartNodeFlags, EndNodeFlags;
            public bool CheckFlags(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags);
            public PropInfoExt(NetLaneProps.Prop template) { }

            public static PropInfoExt Get(NetInfoExtension.Lane.Prop IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex]
                .LaneInfoExts[IndexExt.LaneIndex]
                .PropInfoExts[IndexExt.Index];
            }
        }

        public Version Version;

        public Node[] NodeInfoExts;

        public SegmentInfoExt[] SegmentInfoExts;

        public LaneInfoExt[] LaneInfoExts;

        public NetInfoExt(NetInfo template) {
            Version = HelpersExtensions.VersionOf(this);
            SegmentInfoExts = new SegmentInfoExt[template.m_segments.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                SegmentInfoExts[i] = new SegmentInfoExt(template.m_segments[i]);
            }

            NodeInfoExts = new Node[template.m_nodes.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                NodeInfoExts[i] = new Node(template.m_nodes[i]);
            }

            LaneInfoExts = new LaneInfoExt[template.m_lanes.Length];
            for(int i=0;i<LaneInfoExts.Length;++i) {
                LaneInfoExts[i] = new LaneInfoExt(template.m_lanes[i]);
            }
        }

        public static NetInfoExt[] Buffer;

        public static void Init() => Buffer = new NetInfoExt[PrefabCollection<NetInfo>.PrefabCount()];
    }
}