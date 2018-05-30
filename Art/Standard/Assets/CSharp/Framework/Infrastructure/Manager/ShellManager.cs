using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    public class ShellManager
    {
        public static Transform parent;
        private int m_getCount;
        private int m_freeCount;
        private int m_storeCount;
        private List<GameObject> m_freeList=new List<GameObject>();
        private List<GameObject> m_allList = new List<GameObject>();
        private static ShellManager m_instance;
        public static ShellManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ShellManager();
        }
        ShellManager()
        {
            var obj = new GameObject("[ShellManager]");
            parent = obj.transform;
#if UNITY_EDITOR && !HIDEHIERA
            //obj.hideFlags = HideFlags.HideInHierarchy;
#endif
            parent.gameObject.SetActive(false);
        }
        ~ShellManager()
        {
            //Debug.Log("Dispose ShellManager");
        }
        public void Dispose()
        {
#if UNITY_EDITOR && LOG
            PrintShell();
#endif
            for (int i = 0; i < m_allList.Count; i++)
            {
                UnityEngine.Object.Destroy(m_allList[i]);
            }
            if (parent != null)
            {
                UnityEngine.Object.Destroy(parent.gameObject);
            }
            parent = null;
            m_allList.Clear();
            m_instance = null;
        }
        public static void Store(int count)
        {
            m_instance.m_storeCount += count;
            for (int i = 0; i < count; i++)
            {
                var o=new GameObject();
                o.transform.SetParent(parent);
                m_instance.m_freeList.Add(o);
                m_instance.m_allList.Add(o);
            }
        }
        public static GameObject Get()
        {
            m_instance.m_getCount++;
            if (m_instance.m_freeList.Count>0)
            {
                var l=m_instance.m_freeList[0];
                m_instance.m_freeList.RemoveAt(0);
                return l;
            }
            else
            {
                var o= new GameObject();
                m_instance.m_allList.Add(o);
                return o;
            }
        }
        public static GameObject Get(Transform parent,Vector3 position,Quaternion rotation)
        {
            var o = Get();
            o.transform.SetParent(parent);
            o.transform.localPosition = position;
            o.transform.localRotation = rotation;
            return o;
        }
        public static GameObject Get(Vector3 position,Quaternion rotation,Transform parent)
        {
            var o = Get();
            o.transform.position = position;
            o.transform.rotation = rotation;
            o.transform.SetParent(parent);
            return o;
        }
        public static void Free(GameObject obj)
        {
            m_instance.m_freeCount++;
            m_instance.m_freeList.Add(obj);
            obj.transform.SetParent(parent);
        }
        private void PrintShell()
        {
#if UNITY_EDITOR && LOG
            var directory = Application.persistentDataPath + "/Profile" + "_" + Core.Macro.storeTime + "/";
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            var builder = new System.Text.StringBuilder();

            builder.AppendLine("最大数量(个),获取(次),释放(次),存储(个)");
            builder.AppendLine(m_allList.Count+"," +m_getCount + "," + m_freeCount + "," + m_storeCount);
            string filePath = directory + "/ShellProfile.csv";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }
    }
}
