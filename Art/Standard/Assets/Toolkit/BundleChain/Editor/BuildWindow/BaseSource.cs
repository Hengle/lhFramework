using System;
using System.Collections.Generic;

namespace lhFramework.Tools.Bundle
{
    public class BaseSource
    {
        public int assetId;
        public int oldAssetId;
        public int guid;
        public bool isBaseIdReset;
        public string fileName;
        public string variantName;
        public string mainAssetPath;
        public string mainAssetName;
        public string category;
        public string bundleName;
        public float bundleSize;
        public ESourceState fileState;
        public bool dontDamage;
        public List<int> dependenciedChainList = new List<int>();
        public int dependenciesChain = -1;

        public List<int> fileDependencied = new List<int>();//被依赖列表，被哪些资源依赖
        public List<int> fileDependencies = new List<int>();//依赖列表，依赖哪些资源，
    }
}
