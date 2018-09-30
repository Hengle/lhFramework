using System;
using System.Collections.Generic;
using ProtoBuf;

namespace lhFramework.Infrastructure.Managers
{
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
