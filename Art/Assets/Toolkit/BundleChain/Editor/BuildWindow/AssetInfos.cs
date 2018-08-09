using ProtoBuf;
using System;
using System.Collections.Generic;

namespace lhFramework.Tools.Bundle
{
    [ProtoContract]
    public class AssetInfo
    {
        [ProtoMember(1)]
        public int guid { get; set; }
        [ProtoMember(2)]
        public string bundleName { get; set; }
        [ProtoMember(3)]
        public string variant { get; set; }
        [ProtoMember(4)]
        public string hash { get; set; }
        [ProtoMember(5)]
        public long size { get; set; }
    }
    [ProtoContract]
    public class AssetInfos
    {
        [ProtoMember(1)]
        public List<AssetInfo> infos { get; set; }
        [ProtoMember(2)]
        public string category { get; set; }
        public AssetInfos()
        {
            infos = new List<AssetInfo>();
        }
    }
}
