//#define AB_MODE;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    #pragma warning disable 0649,0067
    using Debug;
    public class ModelManager
    {
        public delegate void StoreHandler();
        public static Transform parent;
        class ModelPool
        {
            public int index;
            public string path;
            public int storeCount;
            public GameObject obj;
#if UNITY_EDITOR && LOG
            public int getCount;
            public int freeCount;
#endif
            private List<IModel> m_freeList;
            public List<IModel> allList;
            private List<IModel> m_usingList;
            public void Initialize(StoreHandler onStoreHandler)
            {
                m_freeList = new List<IModel>(storeCount);
                allList = new List<IModel>(storeCount);
                m_usingList = new List<IModel>(storeCount);
                //External.ExternalManager.LoadAsset(External.EAssetType.Prefab, path, (o) =>
                //{
                //    obj = o as GameObject;
                //    if (obj == null)
                //    {
                //        //storeHandler();
                //        //storeHandler = null;
                //        Log.i(ELogType.Model, path + " not found !");
                //        //return;
                //        obj = new GameObject(path);
                //    }
                //    obj.transform.SetParent(parent);
                //    for (int i = 0; i < storeCount; i++)
                //    {
                //        var ob = CreateObj();
                //        ob.OnFree();
                //        m_freeList.Add(ob);
                //    }
                //    onStoreHandler();
                //    onStoreHandler = null;
                //});
            }
            public IModel Get()
            {
#if UNITY_EDITOR && LOG
                getCount++;
#endif
                if (m_freeList.Count>0)
                {
                    var f = m_freeList[0];
                    m_freeList.RemoveAt(0);
                    f.OnUse();
                    m_usingList.Add(f);
                    return f;
                }
                else
                {
                    var mod= CreateObj();
                    mod.OnUse();
                    m_usingList.Add(mod);
                    allList.Add(mod);
                    return mod;
                }
            }
            public void Update()
            {
                for (int i = 0; i < m_usingList.Count; i++)
                {
                    m_usingList[i].OnUpdate();
                }
            }
            public void Free(IModel obj)
            {
#if UNITY_EDITOR
                if (m_freeList.Contains(obj))
                {
                    Log.i(ELogType.Model, "model has this obj free mulity  =>" + obj.index);
                    return;
                }
#endif
                obj.OnFree();
                m_freeList.Add(obj);
                m_usingList.Remove(obj);
#if UNITY_EDITOR && LOG
                freeCount++;
#endif
            }
            public bool HasObj(int id)
            {
                for (int i = 0; i < allList.Count; i++)
                {
                    if (allList[i].InstanceID() == id)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void Clear()
            {
                for (int i = 0; i < allList.Count; i++)
                {
                    UnityEngine.Object.Destroy(allList[i].gameObject);
                }
                UnityEngine.Object.Destroy(obj);
            }
            private IModel CreateObj()
            {
                var o = UnityEngine.Object.Instantiate(obj);
                IModel mod=null;
                var con = o.GetComponent<MonoBehaviour>();
                if (con is IModel)
                {
                    mod = con as IModel;
                    ((IModel)con).OnCreate();
                }
                else {
                    Log.i(ELogType.Model, "model must addComponent of ModelControl or ModelManual or ModelData=>"+ obj.name);
                }
                o.transform.SetParent(parent);
                mod.index = index;
                allList.Add(mod);
                return mod;
            }
        }
        class WaitFree
        {
            public IModel waitFree;
            public int freeTime;
            public Action freeHandler;
            public void Clear()
            {
                waitFree = null;
                freeTime = 0;
                freeHandler = null;
            }
        }
        private Dictionary<int, ModelPool> m_dic = new Dictionary<int, ModelPool>();
        private List<WaitFree> m_freeWaitList = new List<WaitFree>();
        private List<WaitFree> m_usingWaitList = new List<WaitFree>(20);
#if LOG
        private List<int> m_dontPreloadList = new List<int>();
        private List<int> m_dontConfigList = new List<int>();
        private string m_directory;
#endif
        private static ModelManager m_instance;
        public static ModelManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ModelManager();
        }
        ModelManager()
        {
#if LOG
            if (string.IsNullOrEmpty(m_directory))
            {
                m_directory = Application.persistentDataPath + "/Profile"+ "_" + Macro.storeTime + "/";
                if (!System.IO.Directory.Exists(m_directory))
                {
                    System.IO.Directory.CreateDirectory(m_directory);
                }
            }
#endif
            var obj = new GameObject("[ModelManager]");
            parent = obj.transform;
            parent.transform.position = new Vector3(-10, -10, -10);
#if UNITY_EDITOR && !HIDEHIERA
            //obj.hideFlags = HideFlags.HideInHierarchy;
#endif
            //parent.gameObject.SetActive(false);
        }
        ~ModelManager()
        {
            //Debug.Log("Dispose ModelManager");
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
            {
                UnityEngine.Object.Destroy(parent.gameObject);
            }
            parent = null;
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
                    Free(wait.waitFree);
                    wait.Clear();
                    m_usingWaitList.Add(wait);
                    i--;
                }
            }
            foreach (var item in m_dic)
            {
                item.Value.Update();
            }
        }
        public void SetParent(bool show)
        {
            parent.gameObject.SetActive(show);
        }
        public static void Store(ModelStoreData[] list,Action onStoreOver)
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
        public static void Store(ModelStoreData data,StoreHandler onStoreHandler)
        {
            var pool = new ModelPool();
            pool.index = data.index;
            pool.path = data.path;
            pool.storeCount = data.count;
            m_instance.m_dic.Add(data.index, pool);
            pool.Initialize(onStoreHandler);
        }
        public static IModel Get(int index,Transform parent,Vector3 position,Quaternion rotation)
        {
            var obj = Get(index);
            if (obj!=null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                if (!obj.gameObject.activeSelf) obj.gameObject.SetActive(true);
            }
            return obj;
        }
        public static IModel Get(int index, Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = Get(index);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.transform.SetParent(parent);
                if (!obj.gameObject.activeSelf) obj.gameObject.SetActive(true);
            }
            return obj;
        }
        public static IModel Get(int index)
        {
#if UNITY_EDITOR
            if (m_instance.m_dic.ContainsKey(index))
            {
                return m_instance.m_dic[index].Get();
            }
            else
            {
                Log.i(ELogType.Model, "LaoHan:ModelManager dont has this index;" + index);
//                if (Macro.isDebug)
//                {
//                    if (index > 0)
//                    {
//#if UNITY_EDITOR && LOG
//                        m_instance.m_dontPreloadList.Add(index);
//#endif
//                        var pool = new ModelPool();
//                        pool.index = index;
//                        pool.path = External.ExternalManager.GetSourcePath(index);
//                        pool.storeCount = 1;
//                        m_instance.m_dic.Add(index, pool);
//                        pool.Initialize(() => { });
//                        return Get(index);
//                    }
//                }
                return null;
            }
#else
            return m_instance.m_dic[index].Get();
#endif
        }
        public static void Free(IModel obj,int freeTime=-1,Action onFreeHandler=null)
        {
#if UNITY_EDITOR
            if (!m_instance.m_dic.ContainsKey(obj.index))
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
#if UNITY_EDITOR && LOG
        private void PrintModel()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Modelid,Total,Get,Free,Store");
            int i = 1;
            foreach (var item in m_instance.m_dic)
            {
                builder.AppendLine(i + "," + item.Key +","+item.Value.allList.Count+ "," + item.Value.getCount + "," + item.Value.freeCount+","+item.Value.storeCount);
                i++;
            }
            builder.AppendLine("waitUsingCount:,"+m_usingWaitList.Count+",freeWaitingCount:,"+m_freeWaitList.Count);

            string filePath = m_directory + "/ModelProfile.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontPreload()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Modelid");
            for (int i = 0; i < m_dontPreloadList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontPreloadList[i]);
            }
            string filePath = m_directory + "/ModelDontPreload.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private void PrintDontConfig()
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine(",Modelid");
            for (int i = 0; i < m_dontConfigList.Count; i++)
            {
                builder.AppendLine(i + "," + m_dontConfigList[i]);
            }
            string filePath = m_directory + "/ModelDontConfig.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
#endif
    }
}
