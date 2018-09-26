using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    public enum ESourceState
    {
        None,
        WaitLoad,
        Loading,
        Loaded
    }
    public class SourceData
    {
        public int guid;
        public UnityEngine.Object mainAsset;
        public UnityEngine.Object[] allAssets;
        public string assetPath;
        public ESourceState state;
        public bool toAsync;
        public List<int> dependencieds = new List<int>();
        public AssetBundle bundle;
        private List<DataHandler<UnityEngine.Object>> m_mainCompleteEventHandlers = new List<DataHandler<UnityEngine.Object>>();
        private List<DataHandler<UnityEngine.Object[]>> m_allCompleteEventHandlers = new List<DataHandler<UnityEngine.Object[]>>();
        public void Load()
        {
            if (toAsync)
            {
                state = ESourceState.Loading;
                var request = AssetBundle.LoadFromFileAsync(assetPath);
                request.completed += OnCreateRequest;
            }
            else
            {
                state = ESourceState.Loading;
                bundle = AssetBundle.LoadFromFile(assetPath);
                allAssets = bundle.LoadAllAssets();
                if (allAssets.Length > 0)
                    mainAsset = allAssets[0];
                state = ESourceState.Loaded;
                for (int i = 0; i < m_mainCompleteEventHandlers.Count; i++)
                {
                    m_mainCompleteEventHandlers[i](mainAsset);
                }
                m_mainCompleteEventHandlers.Clear();
                for (int i = 0; i < m_allCompleteEventHandlers.Count; i++)
                {
                    m_allCompleteEventHandlers[i](allAssets);
                }
                m_allCompleteEventHandlers.Clear();
            }
        }
        public bool Unload(int guid)
        {
#if UNITY_EDITOR
            if (dependencieds.Contains(guid))
            {
                dependencieds.Remove(guid);
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "unload dont has this guid");
            }
#else
            dependencieds.Remove(guid);
#endif
            if (dependencieds.Count <= 0)
            {
                return true;
            }
            return false;
        }
        public void UnloadUnused()
        {
            if (dependencieds.Count <= 0)
            {
                bundle.Unload(true);
                bundle = null;
            }
        }
        public void Destroy(int guid)
        {
            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
            if (allAssets != null)
            {
                for (int i = 0; i < allAssets.Length; i++)
                {
                    UnityEngine.Object.Destroy(allAssets[i]);
                }
                allAssets = null;
            }
            if (mainAsset != null)
            {
                UnityEngine.Object.Destroy(mainAsset);
                mainAsset = null;
            }
        }
        public void AddCompleteHandler(DataHandler<UnityEngine.Object> handler)
        {
            m_mainCompleteEventHandlers.Add(handler);
        }
        public void AddCompleteHandler(DataHandler<UnityEngine.Object[]> handler)
        {
            m_allCompleteEventHandlers.Add(handler);
        }
        public void Reset()
        {
            state = ESourceState.None;
            m_allCompleteEventHandlers.Clear();
            assetPath = null;
            toAsync = false;
        }
        private void OnCreateRequest(AsyncOperation async)
        {
            var request = (AssetBundleCreateRequest)async;
            bundle = request.assetBundle;
            var bundleRequest = bundle.LoadAllAssetsAsync();
            bundleRequest.completed += OnBundleRequest;
        }
        private void OnBundleRequest(AsyncOperation async)
        {
            var request = (AssetBundleRequest)async;
            UnityEngine.Object obj = request.asset;
            mainAsset = obj;
            allAssets = request.allAssets;
            state = ESourceState.Loaded;
            for (int i = 0; i < m_mainCompleteEventHandlers.Count; i++)
            {
                m_mainCompleteEventHandlers[i](mainAsset);
            }
            m_mainCompleteEventHandlers.Clear();
            for (int i = 0; i < m_allCompleteEventHandlers.Count; i++)
            {
                m_allCompleteEventHandlers[i](allAssets);
            }
            m_allCompleteEventHandlers.Clear();
        }
    }
    public class BundleSource : ISource
    {
        public event Action<int> loadEventHandler;
        public event Action<int> unLoadEventHandler;
        public event Action<int> destroyEventHandler;
        public event Action<int> loadedEventHandler;
        public event Action<int> addToLoadEventHandler;

        private Dictionary<int, int[]> m_dependenciesChain = new Dictionary<int, int[]>();
        private Dictionary<int, string> m_guidPaths = new Dictionary<int, string>();
        private Dictionary<int, int> m_variantChains = new Dictionary<int, int>();

        public Dictionary<int, SourceData> loadingSources = new Dictionary<int, SourceData>(Const.maxBundleLoading);
        public Dictionary<int, SourceData> waitLoadSources = new Dictionary<int, SourceData>();
        public Dictionary<int, SourceData> usedSources = new Dictionary<int, SourceData>();

        private List<int> m_loadingWaitDeleteSources = new List<int>();
        private Dictionary<int, int> m_waitLoadSourcesIndex = new Dictionary<int, int>();

        /// <summary>
        /// 初始化
        /// </summary>
        void ISource.Initialize()
        {
            //AnalizeSourceTable();
            AnalizeSourceTableBinary();
        }
        /// <summary>
        /// 加载主资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object> loadHandler, EVariantType variant,bool toAsync)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            var chainId = m_variantChains[guid];
            int[] chain = m_dependenciesChain[chainId];
            if (chain.Length == 1)
            {
                int dep = chain[0] / Const.dependLayerDigit;
                int layer = chain[0] % Const.dependLayerDigit;
                if (usedSources.ContainsKey(dep))
                {
                    if (loadHandler != null)
                    {
                        loadHandler(usedSources[dep].mainAsset);
                    }
                    usedSources[dep].dependencieds.Add(guid);
                }
                else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                {
                    waitLoadSources[dep].AddCompleteHandler(loadHandler);
                    waitLoadSources[dep].dependencieds.Add(guid);
                }
                else if (loadingSources.ContainsKey(dep))
                {
                    loadingSources[dep].AddCompleteHandler(loadHandler);
                    loadingSources[dep].dependencieds.Add(guid);
                }
                else
                {
                    string assetPath = null;
#if UNITY_EDITOR
                    if (m_guidPaths.ContainsKey(dep))
                    {
                        assetPath = m_guidPaths[dep];
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                        return;
                    }
#else
                        assetPath= m_guidPath[dep];
#endif
                    m_waitLoadSourcesIndex.Add(dep, layer);
                    SourceData d = new SourceData();
                    d.guid = dep;
                    d.toAsync = toAsync;
                    d.assetPath = assetPath;
                    d.state = ESourceState.WaitLoad;
                    d.AddCompleteHandler(loadHandler);
                    d.dependencieds.Add(guid);
                    waitLoadSources.Add(dep, d);
                }
            }
            else
            {
                int maxWaitInsertIndex = 0;
                for (int i = 0; i < chain.Length; i++)
                {
                    var lDep = chain[i];
                    int dep = lDep / Const.dependLayerDigit;
                    int layer = lDep % Const.dependLayerDigit;
                    if (m_waitLoadSourcesIndex.ContainsKey(dep))
                    {
                        int index = m_waitLoadSourcesIndex[dep];
                        maxWaitInsertIndex = maxWaitInsertIndex < index ? index : maxWaitInsertIndex;
                    }
                }
                for (int i = 0; i < chain.Length; i++)
                {
                    var lDep = chain[i];
                    int dep = lDep / Const.dependLayerDigit;
                    int layer = lDep % Const.dependLayerDigit;
                    if (usedSources.ContainsKey(dep))
                    {
                        if (loadHandler != null)
                        {
                            loadHandler(usedSources[dep].mainAsset);
                        }
                        usedSources[dep].dependencieds.Add(guid);
                    }
                    else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                    {
                        if (i == chain.Length - 1)
                        {
                            waitLoadSources[dep].AddCompleteHandler(loadHandler);
                            waitLoadSources[dep].dependencieds.Add(guid);
                        }
                    }
                    else if (loadingSources.ContainsKey(dep))
                    {
                        if (i == chain.Length - 1)
                        {
                            loadingSources[dep].AddCompleteHandler(loadHandler);
                            loadingSources[dep].dependencieds.Add(guid);
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        string assetPath = null;
#if UNITY_EDITOR
                        if (m_guidPaths.ContainsKey(dep))
                        {
                            assetPath = m_guidPaths[dep];
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                            return;
                        }
#else
                            assetPath= m_guidPath[dep];
#endif
                        m_waitLoadSourcesIndex.Add(dep, maxWaitInsertIndex + layer);
                        SourceData d = new SourceData();
                        d.guid = dep;
                        d.toAsync = toAsync;
                        d.assetPath = assetPath;
                        d.state = ESourceState.WaitLoad;
                        if (i == chain.Length - 1)
                        {
                            d.AddCompleteHandler(loadHandler);
                        }
                        d.dependencieds.Add(guid);
                        waitLoadSources.Add(dep, d);
                    }
                }
            }
            if (loadEventHandler != null)
            {
                loadEventHandler(guid);
            }
        }
        /// <summary>
        /// 加载全部包资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="loadHandler"></param>
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object[]> loadHandler, EVariantType variant,bool toAsync)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            var chainId = m_variantChains[guid];
            int[] chain = m_dependenciesChain[chainId];
            if (chain.Length == 1)
            {
                int dep = chain[0] / Const.dependLayerDigit;
                int layer = chain[0] % Const.dependLayerDigit;
                if (usedSources.ContainsKey(dep))
                {
                    if (loadHandler != null)
                    {
                        loadHandler(usedSources[dep].allAssets);
                    }
                    usedSources[dep].dependencieds.Add(guid);
                }
                else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                {
                    waitLoadSources[dep].AddCompleteHandler(loadHandler);
                    waitLoadSources[dep].dependencieds.Add(guid);
                }
                else if (loadingSources.ContainsKey(dep))
                {
                    loadingSources[dep].AddCompleteHandler(loadHandler);
                    loadingSources[dep].dependencieds.Add(guid);
                }
                else
                {
                    string assetPath = null;
#if UNITY_EDITOR
                    if (m_guidPaths.ContainsKey(dep))
                    {
                        assetPath = m_guidPaths[dep];
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                        return;
                    }
#else
                        assetPath= m_guidPath[dep];
#endif
                    m_waitLoadSourcesIndex.Add(dep, layer);
                    SourceData d = new SourceData();
                    d.guid = dep;
                    d.toAsync = toAsync;
                    d.assetPath = assetPath;
                    d.state = ESourceState.WaitLoad;
                    d.AddCompleteHandler(loadHandler);
                    d.dependencieds.Add(guid);
                    waitLoadSources.Add(dep, d);
                }
            }
            else
            {
                int maxWaitInsertIndex = 0;
                for (int i = 0; i < chain.Length; i++)
                {
                    var lDep = chain[i];
                    int dep = lDep / Const.dependLayerDigit;
                    int layer = lDep % Const.dependLayerDigit;
                    if (m_waitLoadSourcesIndex.ContainsKey(dep))
                    {
                        int index = m_waitLoadSourcesIndex[dep];
                        maxWaitInsertIndex = maxWaitInsertIndex < index ? index : maxWaitInsertIndex;
                    }
                }
                for (int i = 0; i < chain.Length; i++)
                {
                    var lDep = chain[i];
                    int dep = lDep / Const.dependLayerDigit;
                    int layer = lDep % Const.dependLayerDigit;
                    if (usedSources.ContainsKey(dep))
                    {
                        if (loadHandler != null)
                        {
                            loadHandler(usedSources[dep].allAssets);
                        }
                        usedSources[dep].dependencieds.Add(guid);
                    }
                    else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                    {
                        if (i == chain.Length - 1)
                        {
                            waitLoadSources[dep].AddCompleteHandler(loadHandler);
                            waitLoadSources[dep].dependencieds.Add(guid);
                        }
                    }
                    else if (loadingSources.ContainsKey(dep))
                    {
                        if (i == chain.Length - 1)
                        {
                            loadingSources[dep].AddCompleteHandler(loadHandler);
                            loadingSources[dep].dependencieds.Add(guid);
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        string assetPath = null;
#if UNITY_EDITOR
                        if (m_guidPaths.ContainsKey(dep))
                        {
                            assetPath = m_guidPaths[dep];
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                            return;
                        }
#else
                            assetPath= m_guidPath[dep];
#endif
                        m_waitLoadSourcesIndex.Add(dep, maxWaitInsertIndex + layer);
                        SourceData d = new SourceData();
                        d.guid = dep;
                        d.toAsync = toAsync;
                        d.assetPath = assetPath;
                        d.state = ESourceState.WaitLoad;
                        if (i == chain.Length - 1)
                        {
                            d.AddCompleteHandler(loadHandler);
                        }
                        d.dependencieds.Add(guid);
                        waitLoadSources.Add(dep, d);
                    }
                }
            }
            if (loadEventHandler != null)
            {
                loadEventHandler(guid);
            }
        }
        void ISource.Update()
        {
        }
        void ISource.LateUpdate()
        {
            LoadingToUse();
            WaitedToLoading();
        }
        /// <summary>
        /// 释放bundl文件内存镜像
        /// </summary>
        /// <param name="assetId"></param>
        void ISource.UnLoad(int assetId, EVariantType variant)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
#if UNITY_EDITOR
            if (!usedSources.ContainsKey(guid))
            {
                Debug.Log.i(Debug.ELogType.Error, "BundleSource.Unload dont has this assetId:" + assetId);
                return;
            }
#endif
            var chainId = m_variantChains[guid];
            var chain = m_dependenciesChain[chainId];
            for (int i = 0; i < chain.Length; i++)
            {
                var lDep = chain[i];
                int dep = lDep / Const.dependLayerDigit;
                var source = usedSources[dep];
                source.Unload(dep);
            }
            if (unLoadEventHandler != null)
            {
                unLoadEventHandler(guid);
            }
        }
        /// <summary>
        /// 销毁bundle
        /// </summary>
        /// <param name="assetId"></param>
        void ISource.Destroy(int assetId, EVariantType variant)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
#if UNITY_EDITOR
            if (!usedSources.ContainsKey(guid))
            {
                UnityEngine.Debug.LogError("BundleSource.Unload dont has this assetId:" + assetId);
            }
#endif
            var chainId = m_variantChains[guid];
            var chain = m_dependenciesChain[chainId];
            for (int i = 0; i < chain.Length; i++)
            {
                var lDep = chain[i];
                int dep = lDep / Const.dependLayerDigit;
                var source = usedSources[dep];
                if (source.Unload(dep))
                {
                    source.Destroy(dep);
                    usedSources.Remove(dep);
                }
            }
            if (destroyEventHandler != null)
            {
                destroyEventHandler(guid);
            }
        }
        /// <summary>
        /// 释放无用的资源bundle内存镜像
        /// </summary>
        void ISource.UnloadUnusedAsset()
        {
            foreach (var item in usedSources)
            {
                item.Value.UnloadUnused();
            }
        }
        void ISource.Dispose()
        {

        }
        void AnalizeSourceTable()
        {
            string tablePath = Define.sourceTableUrl;
            if (!File.Exists(tablePath))
            {
                Debug.Log.i(Debug.ELogType.Error, "dont has sourceTable: " + tablePath);
            }
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Open))
            {
                using (StreamReader sw = new StreamReader(fileStream))
                {
                    StringBuilder str = new StringBuilder();
                    string s = null;
                    int k = 0;
                    List<int> deps = new List<int>();
                    List<int> variant = new List<int>();
                    List<int> depends = new List<int>();
                    while (!string.IsNullOrEmpty((s = sw.ReadLine())))
                    {
                        int chainId = 0;
                        int assetId = 0;
                        string assetPath = "";
                        int guid = 0;
                        deps.Clear();
                        variant.Clear();
                        str.Clear();
                        for (int i = 0, j = 0; i < s.Length; i++)
                        {
                            char c = s[i];
                            if (i == 0 && c == '&')
                            {
                                k++;
                                break;
                            }
                            if (k == 0)//资源路径
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        assetId = Convert.ToInt32(str.ToString());
                                    }
                                    else if (j == 1)
                                    {
                                        assetPath = Define.sourceBundleUrl + str.ToString();
                                    }
                                    else
                                    {
                                        variant.Add(Convert.ToInt32(str.ToString()));
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        for (int m = 0; m < variant.Count; m++)
                                        {
                                            int variantId = variant[m];
                                            int g = assetId * Const.variantMaxLength + variantId;
                                            string path = assetPath + "." + ((EVariantType)variantId).ToString();
                                            m_guidPaths.Add(g, path);
                                        }
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                            else if (k == 1)//依赖链
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        chainId = Convert.ToInt32(str.ToString());
                                    }
                                    else
                                    {
                                        var split = str.ToString().Split('-');
                                        for (int m = 0; m < split.Length; m++)
                                        {
                                            int g = Convert.ToInt32(split[m]) * Const.dependLayerDigit + (j - 1);
                                            deps.Add(g);
                                        }
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        m_dependenciesChain.Add(chainId, deps.ToArray());
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                            else if (k == 2)//变体数据
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        guid = Convert.ToInt32(str.ToString());
                                    }
                                    else
                                    {
                                        chainId = Convert.ToInt32(str.ToString());
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        m_variantChains.Add(guid, chainId);
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                        }
                    }
                }
            }
        }
        void AnalizeSourceTableBinary()
        {
            string tablePath = Define.sourceTableUrl;
            if (!File.Exists(tablePath))
            {
                Debug.Log.i(Debug.ELogType.Error, "dont has sourceTable: " + tablePath);
            }
            SourceTable sourceTable=null;
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Open))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                sourceTable = Utility.ProtobufUtility.Deserialize<SourceTable>(bytes);
            }
            for (int i = 0; i < sourceTable.guidPaths.Count; i++)
            {
                m_guidPaths.Add(sourceTable.guidPaths[i].guid, Define.sourceBundleUrl+sourceTable.guidPaths[i].path);
            }
            for (int i = 0; i < sourceTable.dependenciesChains.Count; i++)
            {
                List<int> arr = new List<int>();
                for (int j = 0; j < sourceTable.dependenciesChains[i].depends.Count; j++)
                {
                    for (int x = 0; x < sourceTable.dependenciesChains[i].depends[j].deps.Count; x++)
                    {
                        arr.Add(sourceTable.dependenciesChains[i].depends[j].deps[x] * Const.dependLayerDigit + j);
                    }
                }
                m_dependenciesChain.Add(sourceTable.dependenciesChains[i].chainId, arr.ToArray());
            }
            for (int i = 0; i < sourceTable.variantChains.Count; i++)
            {
                m_variantChains.Add(sourceTable.variantChains[i].guid, sourceTable.variantChains[i].chainId);
            }
            sourceTable = null;
        }
        void LoadingToUse()
        {
            foreach (var item in loadingSources)
            {
                if (item.Value.state == ESourceState.Loaded)
                {
                    m_loadingWaitDeleteSources.Add(item.Key);
                }
            }
            for (int i = 0; i < m_loadingWaitDeleteSources.Count; i++)
            {
                int id = m_loadingWaitDeleteSources[i];
                var data = loadingSources[id];
                usedSources.Add(id, data);
                loadingSources.Remove(id);
                if (loadedEventHandler != null)
                {
                    loadedEventHandler(id);
                }
            }
            m_loadingWaitDeleteSources.Clear();
        }
        void WaitedToLoading()
        {
            int minLayer = -1;
            if (m_waitLoadSourcesIndex.Count > 0 && loadingSources.Count < Const.maxBundleLoading)
            {
                foreach (var item in m_waitLoadSourcesIndex)
                {
                    if (minLayer == -1)
                    {
                        minLayer = item.Value;
                    }
                    minLayer = minLayer < item.Value ? minLayer : item.Value;
                }
                foreach (var item in m_waitLoadSourcesIndex)
                {
                    if (item.Value == minLayer)
                    {
                        if (loadingSources.Count < Const.maxBundleLoading)
                        {
                            var guid = item.Key;
                            var s = waitLoadSources[guid];
                            loadingSources.Add(guid, s);
                            s.Load();
                            waitLoadSources.Remove(guid);
                            m_loadingWaitDeleteSources.Add(guid);
                            if (addToLoadEventHandler != null)
                            {
                                addToLoadEventHandler(guid);
                            }
                        }
                        else
                            break;
                    }
                }
                for (int i = 0; i < m_loadingWaitDeleteSources.Count; i++)
                {
                    m_waitLoadSourcesIndex.Remove(m_loadingWaitDeleteSources[i]);
                }
                m_loadingWaitDeleteSources.Clear();
            }
        }
        public void LoadAll(int count,bool toAsync)
        {
            DataHandler<UnityEngine.Object> loadHandler = (o) =>
            {
                //cou++;
                UnityEngine.Debug.Log(o.name);
            };
            int a = 0;
            foreach (var item in m_guidPaths)
            {
                if (a > count)
                {
                    return;
                }
                a++;
                int guid = item.Key;
                if (guid % Const.variantMaxLength != 0) continue;
                var chainId = m_variantChains[guid];
                int[] chain = m_dependenciesChain[chainId];
                if (chain.Length == 1)
                {
                    int dep = chain[0] / Const.dependLayerDigit;
                    int layer = chain[0] % Const.dependLayerDigit;
                    if (usedSources.ContainsKey(dep))
                    {
                        if (loadHandler != null)
                        {
                            loadHandler(usedSources[dep].mainAsset);
                        }
                        usedSources[dep].dependencieds.Add(guid);
                    }
                    else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                    {
                        waitLoadSources[dep].AddCompleteHandler(loadHandler);
                        waitLoadSources[dep].dependencieds.Add(guid);
                    }
                    else if (loadingSources.ContainsKey(dep))
                    {
                        loadingSources[dep].AddCompleteHandler(loadHandler);
                        loadingSources[dep].dependencieds.Add(guid);
                    }
                    else
                    {
                        string assetPath = null;
#if UNITY_EDITOR
                        if (m_guidPaths.ContainsKey(dep))
                        {
                            assetPath = m_guidPaths[dep];
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                            return;
                        }
#else
                        assetPath= m_guidPath[dep];
#endif
                        m_waitLoadSourcesIndex.Add(dep, layer);
                        SourceData d = new SourceData();
                        d.guid = dep;
                        d.assetPath = assetPath;
                        d.toAsync = toAsync;
                        d.state = ESourceState.WaitLoad;
                        d.AddCompleteHandler(loadHandler);
                        d.dependencieds.Add(guid);
                        waitLoadSources.Add(dep, d);
                    }
                }
                else
                {
                    int maxWaitInsertIndex = 0;
                    for (int i = 0; i < chain.Length; i++)
                    {
                        var lDep = chain[i];
                        int dep = lDep / Const.dependLayerDigit;
                        int layer = lDep % Const.dependLayerDigit;
                        if (m_waitLoadSourcesIndex.ContainsKey(dep))
                        {
                            int index = m_waitLoadSourcesIndex[dep];
                            maxWaitInsertIndex = maxWaitInsertIndex < index ? index : maxWaitInsertIndex;
                        }
                    }
                    for (int i = 0; i < chain.Length; i++)
                    {
                        var lDep = chain[i];
                        int dep = lDep / Const.dependLayerDigit;
                        int layer = lDep % Const.dependLayerDigit;
                        if (usedSources.ContainsKey(dep))
                        {
                            if (loadHandler != null)
                            {
                                loadHandler(usedSources[dep].mainAsset);
                            }
                            usedSources[dep].dependencieds.Add(guid);
                        }
                        else if (m_waitLoadSourcesIndex.ContainsKey(dep))
                        {
                            if (i == chain.Length - 1)
                            {
                                waitLoadSources[dep].AddCompleteHandler(loadHandler);
                                waitLoadSources[dep].dependencieds.Add(guid);
                            }
                        }
                        else if (loadingSources.ContainsKey(dep))
                        {
                            if (i == chain.Length - 1)
                            {
                                loadingSources[dep].AddCompleteHandler(loadHandler);
                                loadingSources[dep].dependencieds.Add(guid);
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            string assetPath = null;
#if UNITY_EDITOR
                            if (m_guidPaths.ContainsKey(dep))
                            {
                                assetPath = m_guidPaths[dep];
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Dont has this assetId: " + dep);
                                return;
                            }
#else
                            assetPath= m_guidPath[dep];
#endif
                            m_waitLoadSourcesIndex.Add(dep, maxWaitInsertIndex + layer);
                            SourceData d = new SourceData();
                            d.guid = dep;
                            d.assetPath = assetPath;
                            d.toAsync = toAsync;
                            d.state = ESourceState.WaitLoad;
                            if (i == chain.Length - 1)
                            {
                                d.AddCompleteHandler(loadHandler);
                            }
                            d.dependencieds.Add(guid);
                            waitLoadSources.Add(dep, d);
                        }
                    }
                }
                if (loadEventHandler != null)
                {
                    loadEventHandler(guid);
                }
            }
        }

    }
}