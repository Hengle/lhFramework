using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
#pragma warning disable 0649, 0067
    using Debug;
    public class EffectManager
    {
        public delegate void StoreHandler();
        public delegate void EffectHandler();
        public static Transform parent { get; private set; }
        class EffectPool
        {
            public int index;
            public string path;
            public int storeCount;
            public EEffectType type;
            public GameObject obj;
            private event EffectHandler pauseHandler;
            private event EffectHandler resumeHandler;
            private List<IEffect> m_freeList;
            private List<IEffect> m_usingList;
#if UNITY_EDITOR && LOG
            public int getCount;
            public int freeCount;
            public List<IEffect> allList = new List<IEffect>(100);
#endif
            public void Initialize(StoreHandler onStoreHandler)
            {
                m_freeList = new List<IEffect>(storeCount);
                m_usingList = new List<IEffect>(storeCount);
                //External.ExternalManager.LoadAsset(External.EAssetType.Prefab, path, (o) =>
                //{
                //    obj = o as GameObject;
                //    if (obj == null)
                //    {
                //        //storeHandler();
                //        //storeHandler = null;
                //        Log.i(ELogType.Effect,path + " not found !");
                //        //return;
                //        obj = new GameObject(path);
                //        obj.AddComponent<ParticleSystem>();
                //    }
                //    obj.transform.SetParent(parent);
                //    for (int i = 0; i < storeCount; i++)
                //    {
                //        var  ob=CreateObj();
                //        m_freeList.Add(ob);
                //    }
                //    onStoreHandler();
                //    onStoreHandler = null;
                //});
            }
            public IEffect Get()
            {
                if (m_freeList.Count>0)
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
                    var a=CreateObj();
                    m_usingList.Add(a);
                    return a;
                }
            }
            public void Free(IEffect obj)
            {
                if (m_freeList.Contains(obj)) return;
                m_usingList.Remove(obj);
                m_freeList.Add(obj);
#if UNITY_EDITOR && LOG
                freeCount++;
#endif
            }
            public void Update()
            {
                for (int i = 0; i < m_usingList.Count; i++)
                {
                    m_usingList[i].OnUpdate();
                }
            }
            public bool HasObj(int id)
            {
                for (int i = 0; i < m_usingList.Count; i++)
                {
                    var index=m_usingList[i].gameObject.GetInstanceID();
                    if (index == id)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void Clear()
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
            public void Pause()
            {
                if (pauseHandler!=null)
                    pauseHandler();
            }
            public void Resume()
            {
                if (resumeHandler != null)
                    resumeHandler();
            }
            private IEffect CreateObj()
            {
                var o = GameObject.Instantiate(obj);
                o.transform.SetParent(parent);
                IEffect i;
                if (type==EEffectType.Sub)
                {
#if UNITY_EDITOR
                    var x = o.GetComponent<EffectControl>();
                    if (x != null)
                    {
                        Log.i(ELogType.Error, "特效表中配为Sub(1)方阵特效，prefab上却是普通特效===>" + path + "     " + index);
                    }
#endif
                    SubEffectControl a;
                    a = o.GetComponent<SubEffectControl>();
                    if (a == null)
                    {
                        a = o.AddComponent<SubEffectControl>();
                        Log.i(ELogType.Error, "特效需要挂载  [SubEffectControl]  " + path + "     " + index);
                    }
                    pauseHandler += a.OnPause;
                    resumeHandler += a.OnResume;
                    i = a as IEffect;
                }
                else
                {
#if UNITY_EDITOR
                    var x = o.GetComponent<SubEffectControl>();
                    if (x != null)
                    {
                        Log.i(ELogType.Error, "特效表中配为General(0)普通特效，prefab上却是方阵特效===>" + path + "     " + index);
                    }
#endif
                    EffectControl a;
                    a = o.GetComponent<EffectControl>();
                    if (a == null)
                    {
                        a = o.AddComponent<EffectControl>();
                        Log.i(ELogType.Error, "特效需要挂载  [EffectControl]  " + path + "     " + index);
                    }
                    pauseHandler += a.OnPause;
                    resumeHandler += a.OnResume;
                    i = a as IEffect;
                }
                i.index = index;
                i.effectType = type;
                i.Create();
#if UNITY_EDITOR && LOG
                allList.Add(i);
#endif
                return i;
            }
        }
        class WaitFree
        {
            public IEffect waitFree;
            public int freeTime;
            public Action freeHandler;
            public void Clear()
            {
                waitFree = null;
                freeTime = 0;
                freeHandler = null;
            }
        }
        private Dictionary<int, EffectPool> m_dic = new Dictionary<int, EffectPool>();
        private List<WaitFree> m_freeWaitList = new List<WaitFree>(20);
        private List<WaitFree> m_pausingWaitList = new List<WaitFree>();
        private List<WaitFree> m_usingWaitList = new List<WaitFree>(20);
        private bool m_pause;
#if LOG
        private List<int> m_dontPreloadList = new List<int>();
        private List<int> m_dontConfigList = new List<int>();
        private string m_directory;
#endif
        private static EffectManager m_instance;
        public static EffectManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new EffectManager();
        }
        EffectManager()
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
            var obj = new GameObject("[EffectManager]");
            parent = obj.transform;
#if UNITY_EDITOR && !HIDEHIERA
            //obj.hideFlags = HideFlags.HideInHierarchy;
#endif
            parent.gameObject.SetActive(false);
        }
        ~EffectManager()
        {
            //Log.i(ELogType.Effect,"Dispose EffectManager");
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
            if (parent!=null)
                UnityEngine.Object.Destroy(parent.gameObject);
            parent = null;
            m_instance = null;
        }
        public void Update()
        {
            for (int i = 0; i < m_pausingWaitList.Count; i++)
            {
                var wait = m_pausingWaitList[i];
                wait.freeTime -= (int)TimeManager.deltaTime;
                if (wait.freeTime <= 0)
                {
                    if (wait.freeHandler != null)
                    {
                        wait.freeHandler();
                        wait.freeHandler = null;
                    }
                    m_pausingWaitList.Remove(wait);
                    Free(wait.waitFree);
                    wait.Clear();
                    m_usingWaitList.Add(wait);
                    i--;
                }
            }
            if (!m_pause)
            {
                for (int i = 0; i < m_freeWaitList.Count; i++)
                {
                    var wait = m_freeWaitList[i];
                    wait.freeTime -= (int)TimeManager.deltaTime;
                    if (wait.freeTime <= 0)
                    {
                        if (wait.freeHandler != null)
                        {
                            wait.freeHandler();
                            wait.freeHandler = null;
                        }
                        m_freeWaitList.Remove(wait);
                        Free(wait.waitFree);
                        wait.Clear();
                        m_usingWaitList.Add(wait);
                        i--;
                    }
                }
                var enumer = m_dic.GetEnumerator();
                while (enumer.MoveNext())
                {
                    enumer.Current.Value.Update();
                }
            }
        }
        public void SetParent(bool show)
        {
            //parent.gameObject.SetActive(show);
        }
        public static void Store(EffectStoreData[] list,Action onStoreOver)
        {
            int count = list.Length;
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
            for (int i = 0; i < list.Length; i++)
            {
                var l = list[i];
                Store(l, StoreHandler);
            }
        }
        public static void Store(EffectStoreData data,StoreHandler onStoreHandler)
        {
            var pool = new EffectPool();
            pool.index = data.index;
            pool.path = data.path;
            pool.storeCount = data.count;
            pool.type = (EEffectType)data.type;
            m_instance.m_dic.Add(data.index, pool);
            pool.Initialize(onStoreHandler);
        }
        public static IEffect Get(int index, Transform parent, Vector3 position, Quaternion rotation)
        {
            var obj = Get(index);
            if (obj!=null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
            }
            return obj;
        }
        public static IEffect Get(int index, Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = Get(index);
            if (obj != null)
            {
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                obj.transform.SetParent(parent);
            }
            return obj;
        }
        public static IEffect Get(int mainKey)
        {
            int index = mainKey;
#if UNITY_EDITOR
            if (m_instance.m_dic.ContainsKey(index))
            {
                return m_instance.m_dic[index].Get();
            }
            else
            {
                Debug.Log.i(ELogType.Effect, "LaoHan:EffectManager dont has this index;" + index);
//                if (Macro.isDebug)
//                {
//                    if (index > 0)
//                    {
//#if UNITY_EDITOR && LOG
//                        m_instance.m_dontPreloadList.Add(index);
//#endif
//                        var pool = new EffectPool();
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
        public static void Free(IEffect obj, int freeTime = -1, Action onFreeHandler = null)
        {
#if UNITY_EDITOR
            if (!m_instance.m_dic.ContainsKey(obj.index))
            {
                Log.i(ELogType.Effect, "dont has this index=>" + obj.index);
                if (onFreeHandler != null)
                {
                    onFreeHandler();
                }
                return;
            }
#endif
            if (obj == null)
            {
                if (onFreeHandler!=null)
                {
                    onFreeHandler();
                }
                return;
            }
            if (freeTime > 0)
            {
                WaitFree wait = null;
                if (m_instance.m_usingWaitList.Count <= 0)
                    wait = new WaitFree();
                else
                {
                    wait = m_instance.m_usingWaitList[0];
                    m_instance.m_usingWaitList.RemoveAt(0);
                }
                wait.waitFree = obj;
                wait.freeHandler = onFreeHandler;
                wait.freeTime = freeTime;
                if (m_instance.m_pause)
                    m_instance.m_pausingWaitList.Add(wait);
                else
                    m_instance.m_freeWaitList.Add(wait);
            }
            else
            {
                if (onFreeHandler != null)
                {
                    onFreeHandler();
                }
                m_instance.m_dic[obj.index].Free(obj);
                obj.transform.SetParent(parent);
            }
        }
        public static void Pause()
        {
            m_instance.m_pause = true;
            var enumer = m_instance.m_dic.GetEnumerator();
            while (enumer.MoveNext())
            {
                enumer.Current.Value.Pause();
            }
        }
        public static void Resume()
        {
            m_instance.m_pause = false;
            var enumer = m_instance.m_dic.GetEnumerator();
            while (enumer.MoveNext())
            {
                enumer.Current.Value.Resume();
            }
            for (int i = 0; i < m_instance.m_pausingWaitList.Count; i++)
            {
                var w = m_instance.m_pausingWaitList[i];
                w.waitFree.ResetLayer();
                m_instance.m_freeWaitList.Add(w);
            }
            m_instance.m_pausingWaitList.Clear();
        }
#if UNITY_EDITOR && LOG
        private void PrintModel()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Effectid,Total,Get,Free,Store");
            int i = 1;
            foreach (var item in m_instance.m_dic)
            {
                builder.AppendLine(i + "," + item.Key + "," + item.Value.allList.Count + "," + item.Value.getCount + "," + item.Value.freeCount + "," + item.Value.storeCount);
                i++;
            }
            builder.AppendLine("waitUsingCount:," + m_usingWaitList.Count + ",freeWaitingCount:," + m_freeWaitList.Count);

            string filePath = m_directory + "/EffectProfile.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontPreload()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Effectid");
            for (int i = 0; i < m_dontPreloadList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontPreloadList[i]);
            }
            string filePath = m_directory + "/EffectDontPreload.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontConfig()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Effectid");
            for (int i = 0; i < m_dontConfigList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontConfigList[i]);
            }
            string filePath = m_directory + "/EffectDontConfig.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
#endif
    }
}
