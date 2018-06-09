//#define LOG
using System;
using System.Collections;
using System.Collections.Generic;
#if LOG
using System.IO;
using System.Text;
#endif
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
#pragma warning disable 0649, 0067
    using Debug;
    using Core;
    public class ClassManager
    {
        public static DataHandler<Type> storeHandler;
        public static DataHandler<Type> getHandler;
        public static DataHandler<Type> freeHandler;
        public static DataHandler<Type> clearHandler;
        public static Dictionary<int,ClassPool>[] source { get { return m_instance.m_dic; } }

        private Dictionary<int, ClassPool>[] m_dic;
        private static ClassManager m_instance;
        public static ClassManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ClassManager();
        }
        ClassManager()
        {
            var a = Enum.GetValues(typeof(EClassGroup));
            m_dic = new Dictionary<int, ClassPool>[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                m_dic[i] = new Dictionary<int, ClassPool>();
            }
        }
        ~ClassManager()
        {
            //Log.i(ELogType.Class,"Dispose ClassManager");
        }
        public void Dispose()
        {
            storeHandler = null;
            getHandler = null;
            clearHandler = null;
            freeHandler = null;
            m_instance = null;
        }
        public static void Store(Type type, int count, EClassGroup group=EClassGroup.Base)
        {
            int index = type.MetadataToken;
            var g = m_instance.m_dic[(int)group];
            if (g.ContainsKey(index))
            {
                g[index].Store(count);
            }
            else
            {
                g.Add(index, new ClassPool(type, count,group));
            }
            if (storeHandler != null)
                storeHandler(type);
        }
        public static void Store<T>( int count,EClassGroup group = EClassGroup.Base)
        {
            Type type = typeof(T);
            Store(type, count,group);
        }
        public static IClass Get(string name,EClassGroup group=EClassGroup.Base)
        {
            bool has = false;
            int index = 0;
            IClass cls;
            var g = m_instance.m_dic[(int)group];
            foreach (var item in g)
            {
                if (name == item.Value.typeName)
                {
                    has = true;
                    index = item.Key;
                }
            }
            if (has)
            {
                cls= g[index].GetObject();
            }
            else
            {
                Type type=Type.GetType(name);
                g.Add(index, new ClassPool(type, 1,group));
                cls= g[index].GetObject();
            }
            if (getHandler != null)
                getHandler(cls.GetType());
            return cls;
        }
        public static T Get<T>(EClassGroup group=EClassGroup.Base) where T :IClass
        {
            Type type = typeof(T);
            IClass cls;
            int index = type.MetadataToken;
            var g = m_instance.m_dic[(int)group];
            if (g.ContainsKey(index))
            {
                cls=g[index].GetObject();
            }
            else
            {
                g.Add(index, new ClassPool(type, 1,group));
                cls=g[index].GetObject();
            }
            if (getHandler != null)
                getHandler(cls.GetType());
            return (T)cls;
        }
        public static void Free(IClass obj,EClassGroup group=EClassGroup.Base)
        {
            if (obj == null || m_instance == null) return;
            int index = (int)obj.GetType().MetadataToken;
#if UNITY_EDITOR
            var g = m_instance.m_dic[(int)group];
            if (g.ContainsKey(index))
            {
                g[index].FreeObject(obj);
            }
            else
            {
                Log.i(ELogType.Class, "LaoHan: freeObject dont has this type =>" + obj.GetType());
            }
#else
            m_instance.m_dic[(int)group][index].FreeObject(obj);
#endif
            if (freeHandler != null)
                freeHandler(obj.GetType());
        }
        public void Clear<T>(EClassGroup group)
        {
            var g = m_instance.m_dic[(int)group];
            Type type = typeof(T);
            int index = type.MetadataToken;
            g[index].Clear();
            if (clearHandler != null)
                clearHandler(type);
        }
        public void Clear(EClassGroup group)
        {
            var g = m_instance.m_dic[(int)group];
            foreach (var item in g)
            {
                clearHandler(item.Value.type);
                item.Value.Clear();
            }
        }
    }
}