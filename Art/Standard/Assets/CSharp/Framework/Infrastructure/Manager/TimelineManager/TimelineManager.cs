using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    using Debug;
    using System;
    using UnityEngine.Playables;

    public class TimelineManager
    {
        public delegate void StoreHandler();
        class TimelinePool
        {
            public int index;
            public string path;
            public int storeCount;
            public GameObject obj;
            private List<ITimeline> m_freeList;
            private List<ITimeline> m_usingList;
#if UNITY_EDITOR && LOG
            public int getCount;
            public int freeCount;
            public List<ITimeline> allList = new List<ITimeline>(100);
#endif
            public void Initialize(StoreHandler onStoreHandler)
            {
                m_freeList = new List<ITimeline>(storeCount);
                m_usingList = new List<ITimeline>(storeCount);
                //External.ExternalManager.LoadAsset(External.EAssetType.Prefab, path, (o) =>
                //{
                //    obj = o as GameObject;
                //    if (obj == null)
                //    {
                //        Log.i(ELogType.Timeline, path + " not found !");
                //        obj = new GameObject(path);
                //        obj.AddComponent<PlayableDirector>();
                //    }
                //    obj.transform.SetParent(parent);
                //    for (int i = 0; i < storeCount; i++)
                //    {
                //        var ob = CreateObj();
                //        m_freeList.Add(ob);
                //    }
                //    onStoreHandler();
                //    onStoreHandler = null;
                //});
            }
            public ITimeline Get()
            {
                if (m_freeList.Count > 0)
                {
                    var f = m_freeList[0];
                    m_freeList.RemoveAt(0);
#if UNITY_EDITOR && LOG
                    getCount++;
#endif
                    m_usingList.Add(f);
                    return f;
                }
                else
                {
                    var a = CreateObj();
                    m_usingList.Add(a);
                    return a;
                }
            }
            public void Free(ITimeline timeline)
            {
                if (m_freeList.Contains(timeline)) return;
                m_usingList.Remove(timeline);
                m_freeList.Add(timeline);
#if UNITY_EDITOR && LOG
                freeCount++;
#endif
                timeline.gameObject.transform.SetParent(parent);
            }
            public void Break()
            {
                for (int i = 0; i < m_usingList.Count; i++)
                {
                    if (m_usingList[i].canBreak)
                    {
                        Free(m_usingList[i]);
                        i--;
                    }
                }
            }
            public void Clear()
            {
                try
                {
                    for (int i = 0; i < m_usingList.Count; i++)
                    {
                        UnityEngine.Object.Destroy(m_usingList[i].gameObject);
                    }
                    for (int i = 0; i < m_freeList.Count; i++)
                    {
                        UnityEngine.Object.Destroy(m_freeList[i].gameObject);
                    }
                    UnityEngine.Object.Destroy(obj);
                }
                catch { }
            }
            private ITimeline CreateObj()
            {
                var o = GameObject.Instantiate(obj);
                o.transform.SetParent(parent);
                ITimeline i;
                ITimeline x = o.GetComponent<TimelineControl>();

                if (x == null)
                {
                    Log.i(ELogType.Timeline, "Timeline 必须绑定TimelineControl组件" + path + "     " + index);
                    x = o.AddComponent<TimelineControl>();
                }
                i = x as ITimeline;

                i.index = index;
                i.Create();
#if UNITY_EDITOR && LOG
                allList.Add(i);
#endif
                return i;
            }
        }
        private Dictionary<int, TimelinePool> m_dic = new Dictionary<int, TimelinePool>();
#if LOG
        private List<int> m_dontPreloadList = new List<int>();
        private List<int> m_dontConfigList = new List<int>();
        private string m_directory;
#endif
        public static Transform parent { get; private set; }
        private static TimelineManager m_instance;
        public static TimelineManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new TimelineManager();
        }
        TimelineManager()
        {
#if LOG
            if (string.IsNullOrEmpty(m_directory))
            {
                m_directory = Application.persistentDataPath + "/Profile" + "_" + Macro.storeTime + "/";
                if (!System.IO.Directory.Exists(m_directory))
                {
                    System.IO.Directory.CreateDirectory(m_directory);
                }
            }
#endif
            var obj = new GameObject("[TimelineManager]");
            parent = obj.transform;
            parent.gameObject.SetActive(false);
        }
        public void Dispose()
        {
#if LOG && UNITY_EDITOR
            PrintModel();
            PrintDontPreload();
            PrintDontConfig();
#endif
            foreach (var item in m_dic)
            {
                item.Value.Clear();
            }
            if (parent != null)
                UnityEngine.Object.Destroy(parent.gameObject);
            parent = null;
            m_instance = null;
        }
        public static void Store(TimelineStoreData[] arr, StoreHandler onStoreOver)
        {
            int count = arr.Length;
            if (count <= 0)
            {
                onStoreOver();
                return;
            }
            StoreHandler StoreHandler = () =>
            {
                count--;
                if (count <= 0)
                {
                    onStoreOver();
                    return;
                }
            };
            for (int i = 0; i < arr.Length; i++)
            {
                var l = arr[i];
                Store(l, StoreHandler);
            }
        }
        public static void Store(TimelineStoreData data, StoreHandler onStoreHandler)
        {
            var pool = new TimelinePool();
            pool.index = data.index;
            pool.path = data.path;
            pool.storeCount = data.count;
            m_instance.m_dic.Add(data.index, pool);
            pool.Initialize(onStoreHandler);
        }
        public static ITimeline Get(int index)
        {
            foreach (var item in m_instance.m_dic)
            {
                item.Value.Break();
            }
#if UNITY_EDITOR
            if (m_instance.m_dic.ContainsKey(index))
            {
                return m_instance.m_dic[index].Get();
            }
            else
            {
                Debug.Log.i(ELogType.Timeline, "TimelineManager dont has this index;  " + index);
//                if (Macro.isDebug)
//                {
//                    if (index > 0)
//                    {
//#if UNITY_EDITOR && LOG
//                        m_instance.m_dontPreloadList.Add(index);
//#endif
//                        var pool = new TimelinePool();
//                        pool.index = index;
//                        pool.path = External.ExternalManager.GetSourcePath(index);
//                        pool.storeCount = 1;
//                        m_instance.m_dic.Add(index, pool);
//                        pool.Initialize(() => { });
//                    }
//                }
                return null;
            }
#else
            return m_instance.m_dic[index].Get();
#endif
        }
        public static void Free(ITimeline timeline)
        {
            if (m_instance == null) return;
#if UNITY_EDITOR
            if (!m_instance.m_dic.ContainsKey(timeline.index))
            {
                Log.i(ELogType.Timeline, "dont has this index=>" + timeline.index);
                return;
            }
#endif
            m_instance.m_dic[timeline.index].Free(timeline);
        }
#if UNITY_EDITOR && LOG
        private void PrintModel()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Timelineid,Total,Get,Free,Store");
            int i = 1;
            foreach (var item in m_instance.m_dic)
            {
                builder.AppendLine(i + "," + item.Key + "," + item.Value.allList.Count + "," + item.Value.getCount + "," + item.Value.freeCount + "," + item.Value.storeCount);
                i++;
            }
            string filePath = m_directory + "/TimelineProfile.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontPreload()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",TimelineId");
            for (int i = 0; i < m_dontPreloadList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontPreloadList[i]);
            }
            string filePath = m_directory + "/TimelineDontPreload.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontConfig()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",TimelineId");
            for (int i = 0; i < m_dontConfigList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontConfigList[i]);
            }
            string filePath = m_directory + "/TimelineDontConfig.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
#endif
    }
}