using System;
using System.Collections.Generic;
using UnityEditor;

namespace Framework.Tools.Bundle
{
    public class BundlePack
    {
        public void StartBuild()
        {

        }
        /// <summary>
        /// 单独包体bundle设置
        /// </summary>
        /// <param name="category">分类，文件夹名字小写</param>
        /// <param name="source">单独文件是信息</param>
        public void PackSingle(string category, SourceFile source)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(source.mainAssetPath);
            if (assetImporter != null)
            {
                if (source.fileState == ESourceState.MainBundle || source.fileState == ESourceState.SharedBundle)
                {
                    assetImporter.assetBundleName = source.bundleName;
                    assetImporter.assetBundleVariant = source.variantName;
                }
                else
                {
                    assetImporter.assetBundleName = "";
                }
            }
        }
        /// <summary>
        /// 整包包体设置
        /// </summary>
        /// <param name="category">分类，文件夹名字小写</param>
        /// <param name="source">整包体的信息数据</param>
        public void PackEntire(string category, SourceDirectory source)
        {
            string bundleName = category + "/" + source.fileName;
            foreach (var file in source.filesDic)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(file.Value.mainAssetPath);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = bundleName;
                    assetImporter.assetBundleVariant = source.variantName;
                }
            }
        }
        public void BuildOver()
        {

        }
    }
}
