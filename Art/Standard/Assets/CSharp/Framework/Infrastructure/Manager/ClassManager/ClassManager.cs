//#define LOG
using System;
using System.Collections;
using System.Collections.Generic;
#if LOG
using System.IO;
using System.Text;
#endif
using UnityEngine;

namespace Framework.Infrastructure
{
#pragma warning disable 0649, 0067
    using Debug;
    public class ClassManager
    {
        private class ClassPool
        {
            public Type type;
            public int index;
            public string typeName;
            private List<IClass> m_freeList;
#if UNITY_EDITOR && LOG
            public int getCount;
            public int freeCount;
            public int storeCount;
            private List<IClass> m_allList = new List<IClass>(5);
#endif
            public ClassPool(Type type, int count)
            {
#if UNITY_EDITOR && LOG
                storeCount = count;
#endif
                this.type = type;
                this.typeName = type.Name;
                m_freeList = new List<IClass>(count);
                for (int i = 0; i < count; i++)
                {
                    var cla = Activator.CreateInstance(type) as IClass;
                    m_freeList.Add(cla);
#if UNITY_EDITOR && LOG
                    m_allList.Add(cla);
#endif
                }
            }
            public void Store(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var cla = Activator.CreateInstance(type) as IClass;
                    m_freeList.Add(cla);
#if UNITY_EDITOR && LOG
                    m_allList.Add(cla);
                    WriteClassChange(System.DateTime.Now.ToString("mm月dd日hh时mm分ss秒  "), type.ToString(), m_freeList.Count.ToString(), "预存");

#endif
                }
            }
            public IClass GetObject()
            {
#if LOG
                getCount++;
                WriteClassChange(System.DateTime.Now.ToString("mm月dd日hh时mm分ss秒  "), type.ToString(), m_freeList.Count.ToString(), "获取");
#endif
                if (m_freeList.Count <= 0)
                {
                    var cla = Activator.CreateInstance(type) as IClass;
#if UNITY_EDITOR && LOG
                    m_allList.Add(cla);
#endif
                    return cla;
                }
                else
                {
                    var l = m_freeList[0];
                    m_freeList.RemoveAt(0);
                    return l;
                }
            }
            public void FreeObject(IClass obj)
            {
#if UNITY_EDITOR
                if (m_freeList.Contains(obj))
                {
                    Log.i(ELogType.Class, "Has this Obj=>" + obj);
                    return;
                }
#endif
#if LOG
                freeCount++;
                WriteClassChange(System.DateTime.Now.ToString("mm月dd日hh时mm分ss秒  "), type.ToString(), m_freeList.Count.ToString(), "释放");
#endif
                obj.OnReset();
                m_freeList.Add(obj);
            }
#if UNITY_EDITOR && LOG
            public int GetCount()
            {
                return m_allList.Count;
            }

#endif
            public void Clear()
            {
                m_freeList.Clear();
#if UNITY_EDITOR && LOG
                m_allList.Clear();
#endif
            }
        }
        private Dictionary<int, ClassPool> m_dic = new Dictionary<int, ClassPool>();
#if LOG
        private int count = 0;
        private StringBuilder m_objectChangeBuilder;
        private string m_directory;
        private int m_classChangeRow;
#endif
        private static ClassManager m_instance;
        public static ClassManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ClassManager();
        }
        ClassManager()
        {
#if UNITY_EDITOR && LOG
            m_objectChangeBuilder = new StringBuilder();
#endif

        }
        ~ClassManager()
        {
            //Log.i(ELogType.Class,"Dispose ClassManager");
        }
        public void Dispose()
        {
#if UNITY_EDITOR && LOG
            using (FileStream stream = new FileStream(m_directory + "/ClassMaxCount.csv", FileMode.Append))
            {
                var builder = new StringBuilder();
                builder.AppendLine(" ,类名,最大数量,预存数量,获取次数,释放次数,");
                int i = 1;
                foreach (var item in m_dic)
                {
                    var str = "";
                    if (item.Value.freeCount > item.Value.getCount)
                    {
                        str = "Error";
                    }
                    else if (item.Value.getCount - item.Value.freeCount > 10)
                    {
                        str = "Warning";
                    }
                    builder.AppendLine(i + "," + item.Value.type + "," + item.Value.GetCount() + "," + item.Value.storeCount + "," + item.Value.getCount + "," + item.Value.freeCount + "," + str);
                    i++;
                }
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
            if (m_classChangeRow < 5000)
            {
                SaveClassChange();
            }
            m_objectChangeBuilder = null;
#endif
            Clear();
            m_instance = null;
        }
        public void Update()
        {
#if UNITY_EDITOR && LOG
            if (m_instance != null)
            {
                if (m_classChangeRow > 5000)
                {
                    SaveClassChange();
                }
            }
#endif
        }
        public static void Store(EClassType classType, Type type, int count)
        {
            int index = (int)classType;
            if (m_instance.m_dic.ContainsKey(index))
            {
                m_instance.m_dic[index].Store(count);
            }
            else
            {
                m_instance.m_dic.Add(index, new ClassPool(type, count));
            }
        }
        public static void Store<T>(EClassType classType, int count)
        {
            Type type = typeof(T);
            Store(classType, type, count);
        }
        public static IClass Get(string name)
        {
            bool has = false;
            int index = 0;
            foreach (var item in m_instance.m_dic)
            {
                if (name == item.Value.typeName)
                {
                    has = true;
                    index = item.Key;
                }
            }
            if (has)
            {
                return m_instance.m_dic[index].GetObject();
            }
            else return null;
        }
        public static T Get<T>(EClassType classType) where T : IClass
        {
            return (T)Get(classType);
        }
        public static IClass Get(EClassType classType)
        {
            int index = (int)classType;
            if (m_instance.m_dic.ContainsKey(index))
            {
                return m_instance.m_dic[index].GetObject();
            }
            else
                return null;
        }
        public static void Free(IClass obj)
        {
            if (obj == null || m_instance == null) return;
            int index = (int)obj.classType;
#if UNITY_EDITOR
            if (m_instance.m_dic.ContainsKey(index))
            {
                m_instance.m_dic[index].FreeObject(obj);
            }
            else
            {
                Log.i(ELogType.Class, "LaoHan: freeObject dont has this type =>" + obj.classType);
            }
#else
            m_instance.m_dic[index].FreeObject(obj);
#endif
        }
        public void Clear()
        {
            foreach (var item in m_dic)
            {
                item.Value.Clear();
            }
        }
        private static void WriteClassChange(string time, string classType, string count, string type, bool first = false)
        {
#if LOG
            if (first)
            {
                m_instance.m_objectChangeBuilder.AppendLine("时间,类名,当前数量,方式");
                m_instance.m_objectChangeBuilder.AppendLine(" ," + time + "," + classType + "," + count + "," + type);
            }
            else
                m_instance.m_objectChangeBuilder.AppendLine(m_instance.m_classChangeRow + "," + time + "," + classType + "," + count + "," + type);
            m_instance.m_classChangeRow++;
#endif
        }
        private void SaveClassChange()
        {
#if LOG
            if (string.IsNullOrEmpty(m_directory))
            {
                m_directory = Application.persistentDataPath + "/Profile" + "_" + Macro.storeTime + "/ClassManager/";
                if (!Directory.Exists(m_directory))
                {
                    Directory.CreateDirectory(m_directory);
                }
            }
            var dir = m_directory + "ClassChange/";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream stream = new FileStream(dir + "Class_" + count + ".csv", FileMode.Append))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(m_instance.m_objectChangeBuilder.ToString());
                stream.Write(bytes, 0, bytes.Length);
                m_instance.m_objectChangeBuilder.Length = 0;
                m_classChangeRow = 0;
                count++;
                WriteClassChange("时间", "类名", "当前数量", "方式", true);
            }
#endif
        }
    }
}