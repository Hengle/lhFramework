using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;


using UnityEngine.Networking;

namespace lhFramework.Infrastructure.Managers
{
    using Components;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Utility;
    
    public class HotUpdateManager
    {
        private enum EStatus
        {
            None,
            CheckRootInfos,
            CheckChangedInfos,
            Checked

        }
        private string m_newVersion;
        private string m_newVersionStr;
        private string m_localVersion;
        private int m_major;
        private List<string> m_changedInfos=new List<string>();
        private List<string> m_deletedInfos=new List<string>();
        private Dictionary<string, int> m_localRootInfos = new Dictionary<string, int>();
        private Dictionary<string, int> m_remoteRootInfos = new Dictionary<string, int>();
        private Dictionary<string, AssetInfos> m_remoteChangedAssetInfos = new Dictionary<string, AssetInfos>();
        private Dictionary<string, AssetInfos> m_localChangedAssetInfos = new Dictionary<string, AssetInfos>();
        private List<AssetInfo> m_waitDownloadInfo = new List<AssetInfo>();
        private List<AssetInfo> m_waitDeleteInfo = new List<AssetInfo>();
        private float m_waitDownloadSize;
        private string m_streamingAssetUrl;
        private int m_needDownloadSize;
        private EStatus m_status;
        private int workerThread;
        private int completionPorThread;
        private static HotUpdateManager m_instance;
        public static HotUpdateManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new HotUpdateManager();
        }
        HotUpdateManager()
        {
            m_streamingAssetUrl=Core.Define.streamingAssetUrl;
        }
        public void Dispose()
        {
            m_instance = null;
        }
        public void Update()
        {
            ThreadPool.GetAvailableThreads(out workerThread, out completionPorThread);
            UnityEngine.Debug.Log(workerThread + "   " + completionPorThread);
            if (m_status == EStatus.None) return;
            if (m_status==EStatus.CheckRootInfos)
            {
                CheckRootInfos();
                m_status = EStatus.None;
            }
            else if (m_status==EStatus.CheckChangedInfos)
            {
                CheckChangedInfos();
                m_status = EStatus.None;
            }
            else if (m_status==EStatus.Checked)
            {
                UnityEngine.Debug.Log("m_waitDownloadSize:" + m_waitDownloadSize);
                m_status = EStatus.None;
            }
        }
        public void Check()
        {
            ThreadPool.SetMaxThreads(5, 5);
            m_localVersion = ResourcesManager.LoadFile("version");
            m_localVersion = m_localVersion.TrimEnd();
            var split = m_localVersion.Split('.');
            m_major = Convert.ToInt32(split[0]);
            string url = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/version";
            UnityEngine.Debug.Log(url);
            DownloadBytes download = new DownloadBytes(url, -1, 1, null);
            download.downloadingHandler += delegate(double percent, bool completed, byte[] result)
            {
                if (completed)
                {
                    m_newVersion = System.Text.Encoding.UTF8.GetString(result);
                    m_newVersion = m_newVersion.TrimEnd();
                    m_newVersion = m_newVersion.Replace("\n", "");
                    m_newVersionStr = m_newVersion.Replace('.', '_');
                    m_status = EStatus.CheckRootInfos;
                }
            };
        }
        private void CheckRootInfos()
        {
            string url = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/" + m_newVersionStr + "/" + Core.Const.bundleFolder + "/info";
            DownloadBytes download = new DownloadBytes(url, -1, 1, null);
            download.downloadingHandler += delegate (double percent, bool completed, byte[] result)
            {
                if (completed)
                {
                    if (m_localRootInfos.Count<=0)
                    {
                        LoadLocalRootInfo();
                    }
                    var text = System.Text.Encoding.UTF8.GetString(result);
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
                            m_deletedInfos.Add(remotes.Key);
                        }
                    }
                    m_status = EStatus.CheckChangedInfos;
                }
            };
        }
        private void CheckChangedInfos()
        {
            string hostUrl = Core.Define.host + "/" + Core.Define.platform + "/" + m_major + "/" + m_newVersion.Replace('.', '_') + "/" + Core.Const.bundleFolder + "/";
            int count = 1;
            for (int i = 0; i < 1; i++)
            {
                string url = hostUrl + m_changedInfos[i] + "/info";
                DownloadBytes download = new DownloadBytes(url, -1, 1);
                download.downloadingHandler += delegate (double percent, bool completed, byte[] result)
                {
                    if (completed)
                    {
                        if (m_localChangedAssetInfos.Count<=0)
                        {
                            LoadLocalAssetInfos();
                        }
                        count--;
                        AssetInfos remoteAssetInfos = ProtobufUtility.Deserialize<AssetInfos>(result);
                        m_remoteChangedAssetInfos.Add(remoteAssetInfos.category, remoteAssetInfos);
                        if (count<=0)
                        {
                            CompareWaitDownloads();
                            m_status = EStatus.Checked;
                        }
                    }
                };
                return;
            }
        }
        private void LoadLocalRootInfo()
        {
            var fileStream = ResourcesManager.LoadStream("Info");
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
            fileStream.Dispose();
        }
        private void LoadLocalAssetInfos()
        {
            for (int i = 0; i < m_changedInfos.Count; i++)
            {
                var fileStream = ResourcesManager.LoadStream(m_changedInfos[i]+"/info");
                byte[] bytes = new byte[fileStream.Length];
                AssetInfos localAssetInfos = ProtobufUtility.Deserialize<AssetInfos>(bytes);
                m_localChangedAssetInfos.Add(localAssetInfos.category, localAssetInfos);
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Dispose();
                fileStream = null;
            }
        }
        private void CompareWaitDownloads()
        {
            foreach (var remote in m_remoteChangedAssetInfos)
            {
                var localAssetInfos=m_localChangedAssetInfos[remote.Key];
                var remoteAssetInfos = remote.Value;
                for (int i = 0; i < remoteAssetInfos.infos.Count; i++)
                {
                    bool has = false;
                    for (int j = 0; j < localAssetInfos.infos.Count; j++)
                    {
                        if (localAssetInfos.infos[j].guid== remoteAssetInfos.infos[i].guid)
                        {
                            if (localAssetInfos.infos[j].hash != remoteAssetInfos.infos[i].hash)
                            {
                                m_waitDownloadInfo.Add(remoteAssetInfos.infos[i]);
                                m_waitDownloadSize += remoteAssetInfos.infos[i].size;
                            }
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        m_waitDownloadInfo.Add(remoteAssetInfos.infos[i]);
                        m_waitDownloadSize += remoteAssetInfos.infos[i].size;
                    }
                }
                for (int i = 0; i < localAssetInfos.infos.Count; i++)
                {
                    bool has = false;
                    for (int j = 0; j < remoteAssetInfos.infos.Count; j++)
                    {
                        if (localAssetInfos.infos[i].guid==remoteAssetInfos.infos[j].guid)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        m_waitDeleteInfo.Add(localAssetInfos.infos[i]);
                    }
                }
            }
        }
        private AssetInfos LoadLocalAssetInfos(string category)
        {
            var bytes = ResourcesManager.LoadBytes(category+"/info");
            return ProtobufUtility.Deserialize<AssetInfos>(bytes);
        }
    }
}
