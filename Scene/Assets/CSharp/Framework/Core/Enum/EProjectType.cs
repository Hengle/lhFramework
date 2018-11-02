using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Core
{
    public enum EProjectType
    {
        /// <summary>
        /// 开发模式，读取本地原始文件（非bundle文件）
        /// </summary>
        Development,
        /// <summary>
        /// 调试模式，读取本地streamAssetPath，bundle文件
        /// </summary>
        Debug,
        /// <summary>
        /// 版本模式，读取缓存区资源，bundle文件
        /// </summary>
        Release
    }
}
