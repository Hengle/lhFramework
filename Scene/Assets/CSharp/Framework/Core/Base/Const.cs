using System;
using System.Collections.Generic;


namespace lhFramework.Infrastructure.Core
{
    public class Const
    {
        /// <summary>
        /// 资源bundle父文件夹名字
        /// </summary>
        public const string sourceBundleFolder = "Bundle";
        /// <summary>
        /// 场景bundle父文件夹名字
        /// </summary>
        public const string sceneBundleFolder = "Scene";
        /// <summary>
        /// 资源文件表的名字
        /// </summary>
        public const string sourceTableName = "SourceTable.txt";
        /// <summary>
        /// 场景文件表的名字
        /// </summary>
        public const string sceneTableName = "SceneTable.txt";
        /// <summary>
        /// 同时有多少个bundle文件加载
        /// </summary>
        public const int maxBundleLoading = 10;
        /// <summary>
        /// 依赖优化用的最多有多少个依赖层 最多99个
        /// </summary>
        public const int dependLayerDigit = 100;
        /// <summary>
        /// 资源变体的最大个数（必须是10的倍数）
        /// </summary>
        public const int variantMaxLength = 10;

    }
}
