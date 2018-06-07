using System;
using System.Collections;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    public interface ISource
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        void Update();
        /// <summary>
        /// 刷新逐步加载bundle
        /// </summary>
        void LateUpdate();
        /// <summary>
        /// 加载单个主资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        /// <param name="variant"></param>
        void Load(int assetId, DataHandler<UnityEngine.Object> loadHandler, EVariantType variant );
        /// <summary>
        /// 加载bundle包里所有的资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        /// <param name="variant"></param>
        void Load(int assetId, DataHandler<UnityEngine.Object[]> loadHandler, EVariantType variant);
        /// <summary>
        /// 释放bundle包引用
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="variant"></param>
        void UnLoad(int assetId, EVariantType variant = EVariantType.n);
        /// <summary>
        /// 销毁bundle资源包
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="variant"></param>
        void Destroy(int assetId, EVariantType variant = EVariantType.n);
        /// <summary>
        /// 释放被引用数为0的bundle资源
        /// </summary>
        void UnloadUnusedAsset();
        void Dispose();
    }
}