using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Tools.Bundle
{
    public class CategoryData
    {
        public int baseId;
        public int oldId;
        public bool isBaseIdReset;
        public Vector2 scrollPos;
        public float scrollHeight;
        public float scrollWidth;
        public string category;
        public Dictionary<string, VariantData> variantDic = new Dictionary<string, VariantData>();
    }
}
