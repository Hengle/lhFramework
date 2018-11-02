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
        /// 异步加载单个主资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        /// <param name="variant"></param>
        void Load(int assetId, DataHandler<UnityEngine.Object> loadHandler, EVariantType variant ,bool toAsync);
        /// <summary>
        /// 异步加载bundle包里所有的资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        /// <param name="variant"></param>
        void Load(int assetId, DataHandler<UnityEngine.Object[]> loadHandler, EVariantType variant,bool toAsync);
        /// <summary>
        /// 同步加载bundle所有资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        UnityEngine.Object[] Load(int assetId, EVariantType variant);
        /// <summary>
        /// 同步加载获取bundle单个资源（此方法不能和异步加载同时使用，因为如果加载同一个参数的时候，如果异步加载的资源在等待中或正在加载中会出现报错）
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="name"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        UnityEngine.Object Load(int assetId,string name, EVariantType variant);
        /// <summary>
        /// 释放bundle包引用（此方法不能和异步加载同时使用，因为如果加载同一个参数的时候，如果异步加载的资源在等待中或正在加载中会出现报错）
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