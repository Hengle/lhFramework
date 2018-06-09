//using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
#pragma warning disable 0649, 0067
    using Debug;
    using Core;
    public class EffectPool
    {
        public EEffectGroup group;
        public int index;
        public int storeCount;
        public int capacity;
        public GameObject obj;
        private EventHandler pauseHandler;
        private EventHandler resumeHandler;
        public List<IEffect> freeList=new List<IEffect>();
        public List<IEffect> usingList=new List<IEffect>();
#if UNITY_EDITOR
        public int getCount;
        public int freeCount;
        public List<IEffect> allList = new List<IEffect>(100);
#endif
        public LoadHandler loadHandler;
        public EffectPool()
        {

        }
        public void Initialize(EventHandler onStoreHandler)
        {
            freeList = new List<IEffect>(storeCount);
            usingList = new List<IEffect>(storeCount);
            loadHandler(index, delegate (UnityEngine.Object o)
            {
                obj = o as GameObject;
                if (obj == null)
                {
                    Log.i(ELogType.Effect, index+ " not found !");
                    obj = new GameObject(index.ToString());
                    obj.AddComponent<ParticleSystem>();
                }
                obj.transform.SetParent(EffectManager.parent);
                for (int i = 0; i < storeCount; i++)
                {
                    var ob = CreateObj();
                    freeList.Add(ob);
                }
                if(onStoreHandler!=null)
                    onStoreHandler();
                onStoreHandler = null;
            });
        }
        public void Add(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ob = CreateObj();
                freeList.Add(ob);
            }
        }
        public IEffect Get()
        {
            if (freeList.Count>0)
            {
                var f = freeList[0];
                freeList.RemoveAt(0);
#if UNITY_EDITOR
                getCount++;
#endif
                usingList.Add(f);
                return f;
            }
            else if (capacity>0 && usingList.Count> capacity)
            {
                return null;
            }
            else
            {
                var a=CreateObj();
                usingList.Add(a);
                return a;
            }
        }
        public void Free(IEffect obj)
        {
            if (freeList.Contains(obj)) return;
            usingList.Remove(obj);
            freeList.Add(obj);
#if UNITY_EDITOR
            freeCount++;
#endif
        }
        public void Update()
        {
            for (int i = 0; i < usingList.Count; i++)
            {
                usingList[i].OnUpdate();
            }
        }
        public bool HasObj(int id)
        {
            for (int i = 0; i < usingList.Count; i++)
            {
                var index=usingList[i].gameObject.GetInstanceID();
                if (index == id)
                {
                    return true;
                }
            }
            return false;
        }
        public void Clear()
        {
            for (int i = 0; i < usingList.Count; i++)
            {
                UnityEngine.Object.Destroy(usingList[i].gameObject);
            }
            for (int i = 0; i < freeList.Count; i++)
            {
                UnityEngine.Object.Destroy(freeList[i].gameObject);
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
        public void OnReset()
        {
            index = 0;
            storeCount = 0;
            obj = null;
            pauseHandler = null;
            resumeHandler = null;
            freeList.Clear();
            usingList.Clear();
#if UNITY_EDITOR
            allList.Clear();
            getCount = 0;
            freeCount = 0;
#endif
            loadHandler = null;
        }
        private IEffect CreateObj()
        {
            var o = GameObject.Instantiate(obj);
            o.transform.SetParent(EffectManager.parent);
            IEffect i=null;
            var arr = o.GetComponents<MonoBehaviour>();
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a] is IEffect)
                {
                    i = arr[a] as IEffect;
                    break;
                }
            }
            if (i == null)
            {
                i = o.AddComponent<EffectControl>() as IEffect;
                Log.i(ELogType.Error, "特效表中配为Sub(1)方阵特效，prefab上却是普通特效===>  " + index);
            }
            pauseHandler = i.OnPause;
            resumeHandler = i.OnResume;
            i.index = index;
            i.group = group;
            i.Create();
#if UNITY_EDITOR
            allList.Add(i);
#endif
            return i;
        }
    }
    public class EffectManager
    {
        public static DataHandler<int> getHandler;
        public static DataHandler<int> freeHandler;
        public static DataHandler<int> storeHandler;
        public static DataHandler<int> clearHandler;
        public static Dictionary<int,EffectPool>[] source { get{ return m_instance.m_dic; } }
        public static Transform parent { get; private set; }
        public LoadHandler loadHandler;
        public DestroyHandler destroyHandler;
        class WaitFree
        {
            public IEffect waitFree;
            public int freeTime;
            public EEffectGroup group;
            public EventHandler freeHandler;
            public void Clear()
            {
                waitFree = null;
                freeTime = 0;
                freeHandler = null;
            }
        }
        private Dictionary<int, EffectPool>[] m_dic;
        private List<WaitFree> m_freeWaitList = new List<WaitFree>(20);
        private List<WaitFree> m_pausingWaitList = new List<WaitFree>();
        private List<WaitFree> m_usingWaitList = new List<WaitFree>(20);
        private bool m_pause;
        private static EffectManager m_instance;
        public static EffectManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new EffectManager();
        }
        EffectManager()
        {
            var a=System.Enum.GetValues(typeof(EEffectGroup));
            m_dic = new Dictionary<int, EffectPool>[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                m_dic[i] = new Dictionary<int, EffectPool>();
            }
            var obj = new GameObject("[EffectManager]");
            parent = obj.transform;
            parent.gameObject.SetActive(false);
        }
        ~EffectManager()
        {
            //Log.i(ELogType.Effect,"Dispose EffectManager");
        }
        public void Dispose()
        {
            for (int i = 0; i < m_instance.m_dic.Length; i++)
            {
                var group = m_instance.m_dic[i];
                foreach (var dic in group)
                {
                    dic.Value.Clear();
                    destroyHandler(dic.Key);
                }
            }
            if (parent!=null)
                UnityEngine.Object.Destroy(parent.gameObject);
            parent = null;
            m_instance = null;
            getHandler = null;
            freeHandler = null;
            clearHandler = null;
            storeHandler = null;
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
                    Free(wait.waitFree,-1,wait.group);
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
                        Free(wait.waitFree,-1,wait.group);
                        wait.Clear();
                        m_usingWaitList.Add(wait);
                        i--;
                    }
                }
                for (int i = 0; i < m_instance.m_dic.Length; i++)
                {
                    var group = m_instance.m_dic[i];
                    foreach (var dic in group)
                    {
                        dic.Value.Update();
                    }
                }
            }
        }
        public void SetParent(bool show)
        {
            parent.gameObject.SetActive(show);
        }
        public static void Store(EffectStoreData[] list, EventHandler onStoreOver)
        {
            int count = list.Length;
            if (count <= 0)
            {
                onStoreOver();
                return;
            }
            EventHandler StoreHandler = () =>
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
        public static void Store(int index, int count, EEffectGroup group, int capacity = -1, EventHandler onStoreHandler =null)
        {
            var g = m_instance.m_dic[(int)group];
            if (g.ContainsKey(index))
            {
                g[index].Add(count);
                if (onStoreHandler != null)
                    onStoreHandler();
            }
            else
            {
                var pool = new EffectPool();
                pool.index = index;
                pool.capacity = capacity;
                pool.storeCount = count;
                g.Add(index, pool);
                pool.loadHandler = m_instance.loadHandler;
                pool.group = group;
                pool.Initialize(onStoreHandler);
            }
            if (storeHandler != null)
                storeHandler(index);
        }
        public static void Store(EffectStoreData data, EventHandler onStoreHandler =null)
        {
            Store(data.index, data.count,data.group,data.capacity, onStoreHandler);
        }
        public static IEffect Get(int index, Transform parent, Vector3 position, Quaternion rotation, EEffectGroup group = EEffectGroup.Base)
        {
            var obj = Get(index, group);
            if (obj!=null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
            }
            return obj;
        }
        public static IEffect Get(int index, Vector3 position, Quaternion rotation, Transform parent, EEffectGroup group = EEffectGroup.Base)
        {
            var obj = Get(index, group);
            if (obj != null)
            {
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                obj.transform.SetParent(parent);
            }
            return obj;
        }
        public static IEffect Get(int index,EEffectGroup group=EEffectGroup.Base)
        {
            IEffect ef;
#if UNITY_EDITOR
            if (m_instance.m_dic[(int)group].ContainsKey(index))
            {
                ef= m_instance.m_dic[(int)group][index].Get();
            }
            else
            {
                Debug.Log.i(ELogType.Effect, "LaoHan:EffectManager dont has this index;" + index + "    " + group);
                var pool = new EffectPool();
                pool.index = index;
                pool.storeCount = 1;
                m_instance.m_dic[(int)group].Add(index, pool);
                pool.loadHandler = m_instance.loadHandler;
                pool.group = group;
                pool.Initialize(() => { });
                ef = m_instance.m_dic[(int)group][index].Get();
            }
#else
            ef= m_instance.m_dic[(int)group][index].Get();
#endif
            if (getHandler != null)
                getHandler(index);
            return ef;
        }
        public static void Free(IEffect obj, int freeTime = -1, EEffectGroup group=EEffectGroup.Base, EventHandler onFreeHandler = null)
        {
#if UNITY_EDITOR
            if (!m_instance.m_dic[(int)group].ContainsKey(obj.index))
            {
                Log.i(ELogType.Effect, "dont has this index=>" + obj.index + "    " + group);
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
                wait.group = group;
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
                m_instance.m_dic[(int)group][obj.index].Free(obj);
                obj.transform.SetParent(parent);
                if (freeHandler != null)
                    freeHandler(obj.index);
            }
        }
        public static void Clear(int index,EEffectGroup group=EEffectGroup.Base)
        {
#if UNITY_EDITOR
            if (m_instance.m_dic[(int)group].ContainsKey(index))
            {
                if (clearHandler != null)
                    clearHandler(index);
                m_instance.m_dic[index].Clear();
                m_instance.destroyHandler(index);
            }
            else
            {
                Debug.Log.i(ELogType.Model, "Clear dont has this id:" + index + "    " + group);
            }
#else
            m_instance.m_dic[(int)group][index].Clear();
            m_instance.destroyHandler(index);
#endif

        }
        public static void Clear(EEffectGroup group)
        {
            var g = m_instance.m_dic[(int)group];
            foreach (var item in g)
            {
                if (clearHandler != null)
                    clearHandler(item.Key);
                item.Value.Clear();
                m_instance.destroyHandler(item.Key);
            }
        }
        public static void Pause()
        {
            m_instance.m_pause = true;
            for (int i = 0; i < m_instance.m_dic.Length; i++)
            {
                var group = m_instance.m_dic[i];
                foreach (var dic in group)
                {
                    dic.Value.Pause();
                }
            }
        }
        public static void Resume()
        {
            m_instance.m_pause = false;
            for (int i = 0; i < m_instance.m_dic.Length; i++)
            {
                var group = m_instance.m_dic[i];
                foreach (var dic in group)
                {
                    dic.Value.Resume();
                }
            }
            for (int i = 0; i < m_instance.m_pausingWaitList.Count; i++)
            {
                var w = m_instance.m_pausingWaitList[i];
                w.waitFree.ResetLayer();
                m_instance.m_freeWaitList.Add(w);
            }
            m_instance.m_pausingWaitList.Clear();
        }
    }
}
