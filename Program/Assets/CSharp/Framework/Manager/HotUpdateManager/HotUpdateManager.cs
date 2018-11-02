using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    using Components;
    using System.Threading;
    using Utility;
    using System.Collections;
    using UnityEngine.Networking;
    using System.Threading.Tasks;

    public class HotUpdateManager:Singleton<HotUpdateManager>
    {
        private enum EStatus
        {
            None,
            UncompressFirstPackage,
            CheckRootInfos,
            CheckChangedInfos,
            Checked,
            Downloading,
            Downloaded
        }
        private string m_newVersion;
        private string m_newVersionStr;
        private string m_localVersion;
        private int m_major;
        private List<string> m_changedInfos=new List<string>();
        private List<string> m_deletedInfos=new List<string>();
        private List<string> m_addInfos=new List<string>();
        private Dictionary<string, int> m_localRootInfos = new Dictionary<string, int>();
        private Dictionary<string, int> m_remoteRootInfos = new Dictionary<string, int>();
        private Dictionary<string, AssetInfos> m_remoteChangedAssetInfos = new Dictionary<string, AssetInfos>();
        private Dictionary<string, AssetInfos> m_remoteAddAssetInfos = new Dictionary<string, AssetInfos>();
        private Dictionary<string, AssetInfos> m_localChangedAssetInfos = new Dictionary<string, AssetInfos>();
        private Dictionary<string, List<AssetInfo>> m_waitDownloadInfo = new Dictionary<string, List<AssetInfo>>();
        private Dictionary<string, List<AssetInfo>> m_waitDeleteInfo = new Dictionary<string, List<AssetInfo>>();
        private long m_waitDownloadSize;
        private long m_downloadingSize;
        private string m_streamingAssetUrl;
        private List<string> m_failedChangedInfosUrl = new List<string>();
        private List<string> m_failedAddInfosUrl = new List<string>();
        private Dictionary<string, List<AssetInfo>> m_failedDownloadInfo = new Dictionary<string, List<AssetInfo>>();
        private int m_infosCount;
        private int m_reconnectCount=3;

        /// <summary>
        /// 远程版本获取网络连接失败
        /// </summary>
        public Action<string> remoteVersionNetworkErrorHandler;
        /// <summary>
        /// 远程根info信息获取失败
        /// </summary>
        public Action<string> remoteRootInfoNetworkErrorHandler;
        /// <summary>
        /// 远程变化资源信息获取失败
        /// </summary>
        public Action<string> remoteChangeInfoNetworkErrorHandler;
        /// <summary>
        /// 远程下载资源获取失败
        /// </summary>
        public Action remoteDownloadNetworkErrorHandler;

        /// <summary>
        /// 解压缩进度
        /// </summary>
        public Action<float> uncompressProgressHandler;
        /// <summary>
        /// 需要下载多少资源弹窗回掉
        /// </summary>
        public Action<long> needDownloadHandler;
        /// <summary>
        /// 下载进度
        /// </summary>
        public Action<float> downloadProgressHandler;
        /// <summary>
        /// 下载完成
        /// </summary>
        public Action downloadCompletedHandler;
        /// <summary>
        /// 下载资源合并到缓存区
        /// </summary>
        public Action<float> combineProgressHandler;
        /// <summary>
        /// 合并完成
        /// </summary>
        public Action combineCompleteHandler;
        
        public async override Task Initialize(Action onInitialOver=null)
        {
            m_streamingAssetUrl=Core.Define.streamingAssetUrl;
        }
        public void Update()
        {
        }
        ~HotUpdateManager() {
            UnityEngine.Debug.Log("HotUpdateManager Dispose");
        }

        public async Task CheckVersion()
        {
            if (!ResourcesManager.Exists( "version"))
            {
                await Task.Run(()=> { UncompressFirstPackage(); });
            }
            m_localVersion = ResourcesManager.LoadFile("version");
            m_localVersion = m_localVersion.TrimEnd();
            var split = m_localVersion.Split('.');
            m_major = Convert.ToInt32(split[0]);
            string url = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/version";
            UnityEngine.Debug.Log("localVersion:"+ m_localVersion);
            var www = UnityWebRequest.Get(url);
            www.disposeDownloadHandlerOnDispose = true;
            www.chunkedTransfer = true;
            var request = www.SendWebRequest();
            request.completed += OnRemoteVersion;
        }
        public void Download()
        {
            m_reconnectCount = 3;
            if (m_failedDownloadInfo.Count>0)
            {
                ToDownloadAsset(m_failedDownloadInfo);
            }
            else
                ToDownloadAsset(m_waitDownloadInfo);
        }
        public async Task CombineAsset()
        {
            await Task.Run(() =>
            {
                try
                {
                    string tempDir = Define.tempUrl + "/download/" + m_newVersionStr + "/" + Core.Const.sourceBundleFolder + "/";
                    if (!Directory.Exists(tempDir))
                    {
                        return;
                    }
                    string[] files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        var str = files[i].Replace(tempDir, "");
                        string newStr = Path.Combine(Define.sourceBundleUrl, str);
                        FileInfo newStrInfo = new FileInfo(newStr);
                        if (!newStrInfo.Directory.Exists)
                            newStrInfo.Directory.Create();
                        File.Copy(files[i], newStr, true);
                        File.Delete(files[i]);
                        if (combineProgressHandler != null)
                            combineProgressHandler((float)i /(float)files.Length);
                    }
                    Directory.Delete(Define.tempUrl + "/download/" + m_newVersionStr,true);
                    string newPath = Path.Combine(Define.sourceUrl, "version");
                    using (FileStream fileStream = new FileStream(newPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(m_newVersion);
                        fileStream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch(Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
            });
            if (combineCompleteHandler != null)
                combineCompleteHandler();
        }
        private void ToDownloadAsset(Dictionary<string,List<AssetInfo>> loadDic)
        {
            string hostUrl = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/" + m_newVersionStr + "/" + Core.Const.sourceBundleFolder + "/";
            int count = 0;
            foreach (var item in loadDic)
            {
                count += item.Value.Count;
            }
            if (count <= 0)
            {
                if (downloadCompletedHandler != null)
                    downloadCompletedHandler();
                return;
            }
            foreach (var item in loadDic)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    string url = hostUrl + item.Value[i].bundleName + "." + item.Value[i].variant;
                    string tempPath = Define.tempUrl + "/download/" + m_newVersionStr + "/" + Core.Const.sourceBundleFolder + "/" + item.Value[i].bundleName + "." + item.Value[i].variant;
                    FileInfo fileInfo = new FileInfo(tempPath);
                    if (!fileInfo.Directory.Exists)
                        fileInfo.Directory.Create();
                    UnityWebRequest www = UnityWebRequest.Get(url);
                    var value = item.Value;
                    www.disposeDownloadHandlerOnDispose = true;
                    www.chunkedTransfer = true;
                    long length = fileInfo.Exists ? fileInfo.Length : 0;
                    www.SetRequestHeader("Range", string.Format("bytes={0}-", length));
                    www.downloadHandler = new AssetDownloadHandler(tempPath, length);
                    var request = www.SendWebRequest();
                    request.completed += (obj) =>
                    {
                        count--;
                        var async = obj as UnityWebRequestAsyncOperation;
                        var w = async.webRequest;
                        if (w.isNetworkError)
                        {
                            if (!m_failedDownloadInfo.ContainsKey(w.url))
                                m_failedDownloadInfo.Add(w.url, value);
                        }
                        else
                        {
                            if (m_failedDownloadInfo.ContainsKey(w.url))
                                m_failedDownloadInfo.Remove(w.url);
                            m_downloadingSize += ((AssetDownloadHandler)w.downloadHandler).GetSize();
                            ((AssetDownloadHandler)w.downloadHandler).Release();
                            if (downloadProgressHandler != null)
                                downloadProgressHandler((float)m_downloadingSize/ m_waitDownloadSize);
                            if (count <= 0)
                            {
                                if (m_failedDownloadInfo.Count > 0)
                                {
                                    m_reconnectCount--;
                                    if (m_reconnectCount < 0)
                                    {
                                        if (remoteDownloadNetworkErrorHandler != null)
                                        {
                                            remoteDownloadNetworkErrorHandler();
                                        }
                                    }
                                    else
                                        ToDownloadAsset(m_failedDownloadInfo);
                                }
                                else
                                {
                                    if (downloadCompletedHandler != null)
                                        downloadCompletedHandler();
                                }
                            }
                        }
                        w.Dispose();
                    };
                }
            }
        }
        private async void OnRemoteVersion(UnityEngine.AsyncOperation obj)
        {
            var async = obj as UnityWebRequestAsyncOperation;
            var w = async.webRequest;
            if (w.isNetworkError)
            {
                if (remoteVersionNetworkErrorHandler != null)
                    remoteVersionNetworkErrorHandler(w.error);
                w.Dispose();
            }
            else
            {
                byte[] result = w.downloadHandler.data;
                m_newVersion = System.Text.Encoding.UTF8.GetString(result);
                m_newVersion = m_newVersion.TrimEnd();
                m_newVersion = m_newVersion.Replace("\n", "");
                UnityEngine.Debug.Log("remoteVersion:" + m_newVersion);
                if (m_localVersion == m_newVersion)
                {
                    if (downloadCompletedHandler != null)
                        downloadCompletedHandler();
                    w.Dispose();
                }
                else
                {
                    m_newVersionStr = m_newVersion.Replace('.', '_');
                    w.Dispose();
                    await CheckRootInfos();
                }
            }
        }
        private async Task CheckRootInfos()
        {
            await Task.Run(() => { LoadLocalRootInfo("info"); });
            string url = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/" + m_newVersionStr + "/" + Core.Const.sourceBundleFolder + "/info";
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.disposeDownloadHandlerOnDispose = true;
            www.chunkedTransfer = true;
            var request = www.SendWebRequest();
            request.completed += OnRemoteRootInfos;
        }
        private async void OnRemoteRootInfos(UnityEngine.AsyncOperation obj)
        {
            var async = obj as UnityWebRequestAsyncOperation;
            var w = async.webRequest;
            if (w.isNetworkError)
            {
                if (remoteRootInfoNetworkErrorHandler != null)
                    remoteRootInfoNetworkErrorHandler(w.error);
                w.Dispose();
            }
            else
            {
                var text = System.Text.Encoding.UTF8.GetString(w.downloadHandler.data);
                var ca = text.Split('\n');
                for (int i = 0; i < ca.Length; i++)
                {
                    if (string.IsNullOrEmpty(ca[i])) continue;
                    var spl = ca[i].Split(',');
                    m_remoteRootInfos.Add(spl[0], Convert.ToInt32(spl[1]));
                }
                foreach (var remotes in m_remoteRootInfos)
                {
                    if (m_localRootInfos.ContainsKey(remotes.Key))
                    {
                        if (m_localRootInfos[remotes.Key] < remotes.Value)
                        {
                            m_changedInfos.Add(remotes.Key);
                        }
                    }
                    else
                    {
                        m_addInfos.Add(remotes.Key);
                    }
                }
                foreach (var local in m_localRootInfos)
                {
                    if (!m_remoteRootInfos.ContainsKey(local.Key))
                    {
                        m_deletedInfos.Add(local.Key);
                    }
                }
                w.Dispose();
                await CheckChangedInfos();
            }
        }
        private async Task CheckChangedInfos()
        {
            await Task.Run(()=> { LoadLocalAssetInfos(); });
            string hostUrl = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/" + m_newVersion.Replace('.', '_') + "/" + Core.Const.sourceBundleFolder + "/";
            m_infosCount = m_changedInfos.Count+m_addInfos.Count;
            if (m_infosCount<=0)
            {
                if (downloadCompletedHandler != null)
                    downloadCompletedHandler();
                return;
            }
            RemoteChangeInfo(hostUrl, m_changedInfos,m_failedChangedInfosUrl,m_remoteChangedAssetInfos);
            RemoteChangeInfo(hostUrl, m_addInfos, m_failedAddInfosUrl,m_remoteAddAssetInfos);
        }
        private void RemoteChangeInfo(string hostUrl,List<string> urls,List<string> failedInfos, Dictionary<string,AssetInfos> infos)
        {
            for (int i = 0; i < urls.Count; i++)
            {
                string url = hostUrl + urls[i] + "/info";
                UnityWebRequest www = UnityWebRequest.Get(url);
                www.disposeDownloadHandlerOnDispose = true;
                www.chunkedTransfer = true;
                var request = www.SendWebRequest();
                request.completed += (obj) =>
                {
                    m_infosCount--;
                    var async = obj as UnityWebRequestAsyncOperation;
                    var w = async.webRequest;
                    if (w.isNetworkError)
                    {
                        if (!failedInfos.Contains(w.url))
                            failedInfos.Add(w.url);
                        w.Dispose();
                    }
                    else
                    {
                        if (failedInfos.Contains(w.url))
                            failedInfos.Remove(w.url);
                        AssetInfos remoteAssetInfos = ProtobufUtility.Deserialize<AssetInfos>(w.downloadHandler.data);
                        infos.Add(remoteAssetInfos.category, remoteAssetInfos);
                        if (m_infosCount <= 0)
                        {
                            m_infosCount = m_failedChangedInfosUrl.Count + m_failedAddInfosUrl.Count;
                            if (m_infosCount > 0)
                            {
                                m_reconnectCount--;
                                if (m_reconnectCount < 0)
                                {
                                    if (remoteChangeInfoNetworkErrorHandler != null)
                                        remoteChangeInfoNetworkErrorHandler(w.error);
                                    w.Dispose();
                                    return;
                                }
                                RemoteChangeInfo(hostUrl, m_changedInfos, m_failedChangedInfosUrl, m_remoteChangedAssetInfos);
                                RemoteChangeInfo(hostUrl, m_addInfos, m_failedAddInfosUrl, m_remoteAddAssetInfos);
                            }
                            CompareWaitDownloads();
                            if (needDownloadHandler != null)
                                needDownloadHandler(m_waitDownloadSize);
                            else if (downloadCompletedHandler != null)
                                downloadCompletedHandler();
                            else
                                UnityEngine.Debug.LogError("must add complete handler");
                        }
                        w.Dispose();
                    }
                };
            }
        }
        private void LoadLocalAssetInfos()
        {
            for (int i = 0; i < m_changedInfos.Count; i++)
            {
                var fileStream = ResourcesManager.LoadStream(m_changedInfos[i] + "/info");
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                AssetInfos localAssetInfos = ProtobufUtility.Deserialize<AssetInfos>(bytes);
                m_localChangedAssetInfos.Add(localAssetInfos.category, localAssetInfos);
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }
        }
        private void CompareWaitDownloads()
        {
            foreach (var remote in m_remoteChangedAssetInfos)
            {
                var localAssetInfos = m_localChangedAssetInfos[remote.Key];
                var remoteAssetInfos = remote.Value;
                for (int i = 0; i < remoteAssetInfos.infos.Count; i++)
                {
                    bool has = false;
                    for (int j = 0; j < localAssetInfos.infos.Count; j++)
                    {
                        if (localAssetInfos.infos[j].guid == remoteAssetInfos.infos[i].guid)
                        {
                            if (localAssetInfos.infos[j].hash != remoteAssetInfos.infos[i].hash)
                            {
                                if (!m_waitDownloadInfo.ContainsKey(remoteAssetInfos.category))
                                {
                                    m_waitDownloadInfo.Add(remoteAssetInfos.category, new List<AssetInfo>());
                                }
                                m_waitDownloadInfo[remoteAssetInfos.category].Add(remoteAssetInfos.infos[i]);
                                m_waitDownloadSize += remoteAssetInfos.infos[i].size;
                            }
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        if (!m_waitDownloadInfo.ContainsKey(remoteAssetInfos.category))
                        {
                            m_waitDownloadInfo.Add(remoteAssetInfos.category, new List<AssetInfo>());
                        }
                        m_waitDownloadInfo[remoteAssetInfos.category].Add(remoteAssetInfos.infos[i]);
                        m_waitDownloadSize += remoteAssetInfos.infos[i].size;
                    }
                }
                for (int i = 0; i < localAssetInfos.infos.Count; i++)
                {
                    bool has = false;
                    for (int j = 0; j < remoteAssetInfos.infos.Count; j++)
                    {
                        if (localAssetInfos.infos[i].guid == remoteAssetInfos.infos[j].guid)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        if (!m_waitDeleteInfo.ContainsKey(localAssetInfos.category))
                        {
                            m_waitDeleteInfo.Add(localAssetInfos.category, new List<AssetInfo>());
                        }
                        m_waitDeleteInfo[localAssetInfos.category].Add(localAssetInfos.infos[i]);
                    }
                }
            }
            foreach (var remote in m_remoteAddAssetInfos)
            {
                if (!m_waitDownloadInfo.ContainsKey(remote.Value.category))
                {
                    m_waitDownloadInfo.Add(remote.Value.category, new List<AssetInfo>());
                }
                for (int i = 0; i < remote.Value.infos.Count; i++)
                {
                    m_waitDownloadInfo[remote.Value.category].Add(remote.Value.infos[i]);
                    m_waitDownloadSize += remote.Value.infos[i].size;
                }
            }
        }
        private void UncompressFirstPackage()
        {
            string sourcePath = Define.streamingAssetUrl + Define.platform;
            if (File.Exists(sourcePath + "/version"))
            {
                //原bundle文件copy到
                FileUtility.CopyEntireDir(sourcePath, Define.persistentUrl + Define.platform + "/");
            }
            else
            {
                //压缩文件解压到缓存
            }
        }


        private AssetInfos LoadLocalAssetInfo(string category)
        {
            var bytes = ResourcesManager.LoadBytes(category + "/info");
            return ProtobufUtility.Deserialize<AssetInfos>(bytes);
        }
        private void LoadLocalRootInfo(string path)
        {
            var fileStream = ResourcesManager.LoadStream(path);
            using (StreamReader sr = new StreamReader(fileStream))
            {
                string s;
                StringBuilder str = new StringBuilder();
                while ((s = sr.ReadLine()) != null)
                {
                    str.Clear();
                    string categoryName = "";
                    int index = 0;
                    for (int i = 0, j = 0; i < s.Length; i++)
                    {
                        char c = s[i];
                        if (c == ',' || i == s.Length - 1)
                        {
                            if (j == 0)
                            {
                                categoryName = str.ToString();
                            }
                            else if (j == 1)
                            {
                                str.Append(c);
                                index = Convert.ToInt32(str.ToString());
                            }
                            str.Clear();
                            j++;
                        }
                        else
                        {
                            str.Append(c);
                        }
                    }
                    m_localRootInfos.Add(categoryName, index);
                }
            }
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}
