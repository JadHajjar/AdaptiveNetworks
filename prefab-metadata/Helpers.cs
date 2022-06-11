namespace PrefabMetadata.Helpers {
    using PrefabMetadata.API;
    using PrefabMetadata.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class PrefabMetadataHelpers {
        /// <summary>
        /// returns the latest version of PrefabMetadata.dll in the app domain
        /// </summary>
        public static Assembly GetLatestAssembly(bool throwOnError = true) {
            Assembly ret = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                if (assembly.GetName().Name != "PrefabMetadata")
                    continue;
                if (ret == null || ret.GetName().Version < assembly.GetName().Version) {
                    ret = assembly;
                }
            }
            if (ret == null && throwOnError) {
                string sAssemblies = string.Join("\n", assemblies.Select(asm => asm.ToString()).ToArray());
                throw new Exception("failed to get latest PrefabMetadata. assemblies are:\n" + sAssemblies);
            }
            return ret;
        }


        #region segment
        public delegate IInfoExtended<NetInfo.Segment> ExtendSegmentHandler(NetInfo.Segment info);
        public static ExtendSegmentHandler ExtendSegmentDelegate {
            get {
                MethodInfo m =
                     GetLatestAssembly()
                    .GetType(typeof(NetInfoMetaDataExtension.Segment).FullName, throwOnError: true)
                    .GetMethod(nameof(NetInfoMetaDataExtension.Segment.Extend), BindingFlags.NonPublic | BindingFlags.Static);
                if (m == null) throw new Exception("could not get NetInfoMetaDataExtension.Segment.Extend()");
                return (ExtendSegmentHandler)Delegate.CreateDelegate(typeof(ExtendSegmentHandler), m);
            }
        }
        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetInfo.Segment> Extend(this NetInfo.Segment info) =>
            ExtendSegmentDelegate(info);
        #endregion segment

        #region node
        public delegate IInfoExtended<NetInfo.Node> ExtendNodeHandler(NetInfo.Node info);
        public static ExtendNodeHandler ExtendNodeDelegate {
            get {
                MethodInfo m =
                     GetLatestAssembly()
                    .GetType(typeof(NetInfoMetaDataExtension.Node).FullName, throwOnError: true)
                    .GetMethod(nameof(NetInfoMetaDataExtension.Node.Extend), BindingFlags.NonPublic | BindingFlags.Static);
                if (m == null) throw new Exception("could not get NetInfoMetaDataExtension.Segment.Node()");
                return (ExtendNodeHandler)Delegate.CreateDelegate(typeof(ExtendNodeHandler), m);
            }
        }
        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetInfo.Node> Extend(this NetInfo.Node info) =>
            ExtendNodeDelegate(info);
        #endregion node

        #region lane prop
        public delegate IInfoExtended<NetLaneProps.Prop> ExtendPropHandler(NetLaneProps.Prop info);
        public static ExtendPropHandler ExtendPropDelegate {
            get {
                MethodInfo m =
                    GetLatestAssembly()
                   .GetType(typeof(NetInfoMetaDataExtension.LaneProp).FullName, throwOnError: true)
                   .GetMethod(nameof(NetInfoMetaDataExtension.LaneProp.Extend), BindingFlags.NonPublic | BindingFlags.Static);
                return (ExtendPropHandler)Delegate.CreateDelegate(typeof(ExtendPropHandler), m);
            }
        }
        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetLaneProps.Prop> Extend(this NetLaneProps.Prop info) =>
            ExtendPropDelegate(info);
        #endregion lane props

        /// <summary>
        /// returns an extended clone of <paramref name="info"/> that can accept metadata.
        /// returns null if info is unsupported type.
        /// </summary>
        public static IInfoExtended Extend(object info) {
            if (info.GetType() == typeof(NetInfo.Segment))
                return Extend(info as NetInfo.Segment);
            if (info.GetType() == typeof(NetInfo.Node))
                return Extend(info as NetInfo.Node);
            if (info.GetType() == typeof(NetLaneProps.Prop))
                return Extend(info as NetLaneProps.Prop);
            return null;
        }


        public static MetaDataType GetMetaData<MetaDataType>(this IInfoExtended info)
            where MetaDataType : class {
            var list = info.MetaData;
            if (list != null) {
                int n = list.Count;
                for (int i = 0; i < n; ++i) {
                    if (list[i] is MetaDataType ret)
                        return ret;
                }
            }
            return null;
        }

        /// <summary>
        /// adds <paramref name="data"/> to <paramref name="info"/>.
        /// if <paramref name="info"/> already has a meta data of the same type, it will be replaced.
        /// if <paramref name="data"/> is null, any item of type MetaDataType or null will be removed.
        /// </summary>
        public static void SetMetaData<MetaDataType>(this IInfoExtended info, MetaDataType data)
            where MetaDataType : class, ICloneable {
            RemoveMetaData<MetaDataType>(info);
            info.MetaData ??= new List<ICloneable>();
            info.MetaData.Add(data);
        }

        /// <summary>
        /// removes any item of type MetaDataType or null from <paramref name="info"/>
        /// </summary>
        public static void RemoveMetaData<MetaDataType>(this IInfoExtended info)
            where MetaDataType : class, ICloneable {
            if (info.MetaData == null) return;
            bool predicate(ICloneable _m) => _m is null || _m is MetaDataType;
            for (int i = 0; i < info.MetaData.Count; ) {
                if (predicate(info.MetaData[i])) {
                    info.MetaData.RemoveAt(i);
                } else {
                    i++;
                }
            }
        }

        public static List<ICloneable> Clone(this List<ICloneable> list) {
            var ret = new List<ICloneable>(list);
            for (int i = 0; i < list.Count; ++i) {
                ret[i] = ret[i]?.Clone() as ICloneable;
            }
            return ret;
        }
    }
}

