using System;
using System.Collections.Generic;

namespace lhFramework.Tools.Bundle
{
    public class SourceDirectory : BaseSource
    {
        public bool isOpen;
        public Dictionary<string, SourceFile> filesDic = new Dictionary<string, SourceFile>();
    }
}
