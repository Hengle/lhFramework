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
    [ProtoContract]
    public class GuidPath
    {
        [ProtoMember(1)]
        public int guid { get; set; }
        [ProtoMember(2)]
        public string path { get; set; }
    }
    [ProtoContract]
    public class Depends
    {
        [ProtoMember(1)]
        public List<int> deps { get; set; }
        public Depends()
        {
            deps = new List<int>();
        }
    }
    [ProtoContract]
    public class DependenciesChain
    {
        [ProtoMember(1)]
        public int chainId { get; set; }
        [ProtoMember(2)]
        public List<Depends> depends { get; set; }
        public DependenciesChain()
        {
            depends = new List<Depends>();
        }
    }
    [ProtoContract]
    public class VariantChain
    {
        [ProtoMember(1)]
        public int guid { get; set; }
        [ProtoMember(2)]
        public int chainId { get; set; }
    }
    [ProtoContract]
    public class SourceTable
    {
        [ProtoMember(1)]
        public List<GuidPath> guidPaths { get; set; }
        [ProtoMember(2)]
        public List<DependenciesChain> dependenciesChains { get; set; }
        [ProtoMember(3)]
        public List<VariantChain> variantChains { get; set; }
        public SourceTable()
        {
            guidPaths = new List<GuidPath>();
            dependenciesChains = new List<DependenciesChain>();
            variantChains = new List<VariantChain>();
        }
    }
}
