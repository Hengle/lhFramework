using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Managers
{
    using Debug;
    public class ClassPool
    {
        public Type type;
        public int index;
        public EClassGroup group;
        public string typeName;
        public List<IClass> freeList=new List<IClass>();
#if UNITY_EDITOR
        public int getCount;
        public int freeCount;
        public int storeCount;
        public List<IClass> allList = new List<IClass>(5);
#endif
        public ClassPool()
        {

        }
        public ClassPool(Type type, int count,EClassGroup group)
        {
#if UNITY_EDITOR
            storeCount = count;
#endif
            this.group = group;
            index = type.MetadataToken;
            this.type = type;
            this.typeName = type.Name;
            freeList = new List<IClass>(count);
            for (int i = 0; i < count; i++)
            {
                var cla = Activator.CreateInstance(type) as IClass;
                freeList.Add(cla);
#if UNITY_EDITOR
                allList.Add(cla);
#endif
            }
        }
        public void Store(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var cla = Activator.CreateInstance(type) as IClass;
                freeList.Add(cla);
#if UNITY_EDITOR
                allList.Add(cla);
#endif
            }
        }
        public IClass GetObject()
        {
#if UNITY_EDITOR
            getCount++;
#endif
            if (freeList.Count <= 0)
            {
                var cla = Activator.CreateInstance(type) as IClass;
#if UNITY_EDITOR
                allList.Add(cla);
#endif
                return cla;
            }
            else
            {
                var l = freeList[0];
                freeList.RemoveAt(0);
                return l;
            }
        }
        public void FreeObject(IClass obj)
        {
#if UNITY_EDITOR
            if (freeList.Contains(obj))
            {
                Log.i(ELogType.Class, "Has this Obj=>" + obj);
                return;
            }
            freeCount++;
#endif
            obj.OnReset();
            freeList.Add(obj);
        }
#if UNITY_EDITOR
        public int GetCount()
        {
            return allList.Count;
        }

#endif
        public void Clear()
        {
            freeList.Clear();
#if UNITY_EDITOR
            allList.Clear();
#endif
        }
        public void Reset()
        {
            type = null;
            index = 0;
            typeName = null;
            if (freeList!=null)
            {
                freeList.Clear();
            }
#if UNITY_EDITOR
            if (allList!=null)
            {
                allList.Clear();
            }
#endif
        }
    }
}
