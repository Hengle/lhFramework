using System;
using System.Collections.Generic;

namespace lhFramework.Tools.Bundle
{
    public class VariantData
    {
        public string variantName;
        public bool isOpen;
        public Dictionary<string, SourceFile> filesDic = new Dictionary<string, SourceFile>();
        public Dictionary<string, SourceDirectory> directoriesDic = new Dictionary<string, SourceDirectory>();
    }
}
