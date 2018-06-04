//#define AB_MODE;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    #pragma warning disable 0649,0067
    using Debug;
    using Core;
    public class ModelPool
    {
        public int index;
        public int storeCount;
        public EModelGroup group;
        public GameObject obj;
        public List<IModel> usingList=new List<IModel>();
        public List<IModel> freeList=new List<IModel>();
        public LoadHandler loadHandler;

#if UNITY_EDITOR
        public int getCount;
        public int freeCount;
        public List<IModel> allList=new List<IModel>();
#endif
        public ModelPool()
        {

        }
        public void Initialize(EventHandler onStoreHandler)
        {
            freeList = new List<IModel>(storeCount);
#if UNITY_EDITOR
            allList = new List<IModel>(storeCount);
#endif
            usingList = new List<IModel>(storeCount);
            loadHandler(index, delegate (UnityEngine.Object o)
            {
                obj = o as GameObject;
                if (obj == null)
                {
                    Log.i(ELogType.Model, index+ " not found !");
                    obj = new GameObject(index.ToString());
                }
                obj.transform.SetParent(ModelManager.parent);
                for (int i = 0; i < storeCount; i++)
                {
                    var ob = CreateObj();
                    freeList.Add(ob);
                }
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
        public IModel Get()
        {
#if UNITY_EDITOR
            getCount++;
#endif
            if (freeList.Count > 0)
            {
                var f = freeList[0];
                freeList.RemoveAt(0);
                f.OnUse();
                usingList.Add(f);
                return f;
            }
            else
            {
                var mod = CreateObj();
                mod.OnUse();
                usingList.Add(mod);
#if UNITY_EDITOR
                allList.Add(mod);
#endif
                return mod;
            }
        }
        public void Update()
        {
            for (int i = 0; i < usingList.Count; i++)
            {
                usingList[i].OnUpdate();
            }
        }
        public void Free(IModel obj)
        {
#if UNITY_EDITOR
            if (freeList.Contains(obj))
            {
                Log.i(ELogType.Model, "model has this obj free mulity  =>" + obj.index);
                return;
            }
#endif
            obj.OnFree();
            freeList.Add(obj);
            usingList.Remove(obj);
#if UNITY_EDITOR
            freeCount++;
#endif
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
        public void OnReset()
        {
            index = 0;
            storeCount = 0;
#if UNITY_EDITOR
            allList.Clear();
            getCount = 0;
            freeCount = 0;
#endif
            freeList.Clear();
            usingList.Clear();
            loadHandler = null;
            obj = null;
        }
        private IModel CreateObj()
        {
            var o = UnityEngine.Object.Instantiate(obj);
            IModel mod = null;
            var arr = o.GetComponents<MonoBehaviour>();
            for (int i = 0; i < arr.Length; i++)
            {
                if(arr[i] is IModel)
                {
                    mod = arr[i] as IModel;
                    break;
                }
            }
            if (mod == null)
            {
                mod = o.AddComponent<ModelControl>() as IModel;
                Log.i(ELogType.Model, "model must addComponent of ModelControl or ModelManual or ModelData=>" + obj.name);
            }
            mod.OnCreate();
            o.transform.SetParent(ModelManager.parent);
            mod.index = index;
            mod.group = group;
#if UNITY_EDITOR
            allList.Add(mod);
#endif
            return mod;
        }
    }
    public class ModelManager
    {
        public static DataHandler<int> getHandler;
        public static DataHandler<int> freeHandler;
        public static DataHandler<int> storeHandler;
        public static DataHandler<int> clearHandler;
        public LoadHandler loadHandler;
        public DestroyHandler destroyHandler;
        public static Transform parent;
        public static Dictionary<int, ModelPool>[] source { get{ return m_instance.m_dic; } }
        class WaitFree
        {
            public IModel waitFree;
            public int freeTime;
            public EModelGroup group;
            public EventHandler freeHandler;
            public void Clear()
            {
                waitFree = null;
                freeTime = 0;
                freeHandler = null;
            }
        }
        private Dictionary<int, ModelPool>[] m_dic;
        private List<WaitFree> m_freeWaitList = new List<WaitFree>();
        private List<WaitFree> m_usingWaitList = new List<WaitFree>(20);
        private static ModelManager m_instance;
        public static ModelManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ModelManager();
        }
        ModelManager()
        {
            var a = Enum.GetValues(typeof(EModelGroup));
            m_dic = new Dictionary<int, ModelPool>[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                m_dic[i] = new Dictionary<int, ModelPool>();
            }
            var obj = new GameObject("[ModelManager]");
            parent = obj.transform;
            parent.transform.position = new Vector3(-10, -10, -10);
            obj.SetActive(false);
        }
        ~ModelManager()
        {
            //Debug.Log("Dispose ModelManager");
        }
        public void Dispose()
        {
            for (int i = 0; i < m_dic.Length; i++)
            {
                var dic = m_dic[i];
                foreach (var item in dic)
                {
                    item.Value.Update();
                    destroyHandler(item.Key);
                }
            }
            if (parent!=null)
            {
                UnityEngine.Object.Destroy(parent.gameObject);
            }
            parent = null;
            getHandler = null;
            freeHandler = null;
            clearHandler = null;
            storeHandler = null;
            m_instance = null;
        }
        public void Update()
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
            for (int i = 0; i < m_dic.Length; i++)
            {
                var dic = m_dic[i];
                foreach (var item in dic)
                {
                    item.Value.Update();
                }
            }
        }
        public void SetParent(bool show)
        {
            parent.gameObject.SetActive(show);
        }
        public static void Store(ModelStoreData[] list, EventHandler onStoreOver)
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
                Store(l.index,l.count,l.group, StoreHandler);
            }
        }
        public static void Store(int id,int count,EModelGroup group, EventHandler onStoreHandler)
        {
            var g = m_instance.m_dic[(int)group];
            if (g.ContainsKey(id))
            {
                g[id].Add(count);
                onStoreHandler();
            }
            else
            {
                var pool = new ModelPool();
                pool.index = id;
                pool.storeCount = count;
                pool.group = group;
                g.Add(id, pool);
                pool.loadHandler = m_instance.loadHandler;
                pool.Initialize(onStoreHandler);
            }
        }
        public static IModel Get(int index,Transform parent,Vector3 position,Quaternion rotation,EModelGroup group=EModelGroup.Base)
        {
            var obj = Get(index,group);
            if (obj!=null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                if (!obj.gameObject.activeSelf) obj.gameObject.SetActive(true);
            }
            return obj;
        }
        public static IModel Get(int index, Vector3 position, Quaternion rotation, Transform parent, EModelGroup group=EModelGroup.Base)
        {
            var obj = Get(index,group);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.transform.SetParent(parent);
                if (!obj.gameObject.activeSelf) obj.gameObject.SetActive(true);
            }
            return obj;
        }
        public static IModel Get(int index, EModelGroup group = EModelGroup.Base)
        {
            IModel mod;
#if UNITY_EDITOR
            if (m_instance.m_dic[(int)group].ContainsKey(index))
            {
                mod= m_instance.m_dic[(int)group][index].Get();
            }
            else
            {
                Log.i(ELogType.Model, "LaoHan:ModelManager dont has this index;" + index);
                var pool = new ModelPool();
                pool.index = index;
                pool.storeCount = 1;
                m_instance.m_dic[(int)group].Add(index, pool);
                pool.loadHandler = m_instance.loadHandler;
                pool.Initialize(() => { });
                mod= Get(index,group);
            }
#else
            mod= m_instance.m_dic[index].Get();
#endif
            if (getHandler != null)
                getHandler(index);
            return mod;
        }
        public static void Free(IModel obj,int freeTime=-1,EModelGroup group=EModelGroup.Base, EventHandler onFreeHandler=null)
        {
#if UNITY_EDITOR
            if (!m_instance.m_dic[(int)group].ContainsKey(obj.index))
            {
                Log.i(ELogType.Model, "dont has this index=>"+obj.index);
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
        public static void Clear(int index,EModelGroup group=EModelGroup.Base)
        {
#if UNITY_EDITOR
            if (m_instance.m_dic[(int)group].ContainsKey(index))
            {
                if (clearHandler != null)
                    clearHandler(index);
                m_instance.m_dic[(int)group][index].Clear();
                m_instance.destroyHandler(index);
            }
            else
            {
                Debug.Log.i(ELogType.Model, "Clear dont has this id:" + index);
            }
#else
            m_instance.m_dic[(int)group][index].Clear();
            m_instance.destroyHandler(index);
#endif

        }
        public static void Clear(EModelGroup group)
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
    }
}
