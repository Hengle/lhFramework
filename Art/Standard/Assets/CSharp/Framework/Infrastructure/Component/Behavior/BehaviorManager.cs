using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    public class BehaviorManager
    {
        private Dictionary<int, BehaviorData> m_dic = new Dictionary<int, BehaviorData>();
        private List<BehaviorTree> m_usingTree = new List<BehaviorTree>();

        private static BehaviorManager m_instance;
        public static BehaviorManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new BehaviorManager();
        }
        BehaviorManager()
        {
        }
        ~BehaviorManager()
        {

        }
        public void Dispose()
        {
            m_instance = null;
        }
        public void OnDrawGizmos()
        {
            for (int i = 0; i < m_usingTree.Count; i++)
            {
                m_usingTree[i].OnDrawGizmos();
            }
        }
        public void OnDrawGizmosSelected(int id)
        {
            for (int i = 0; i < m_usingTree.Count; i++)
            {
                if (m_usingTree[i].mark == id)
                {
                    m_usingTree[i].OnDrawGizmosSelected();
                }
            }
        }
        public static void Store(Dictionary<int, string> pathDic, Action onStoreOver)
        {
            if (pathDic.Count <= 0)
            {
                onStoreOver();
                return;
            }
            int count = pathDic.Count;
            var enumer = pathDic.GetEnumerator();
            while (enumer.MoveNext())
            {
                var path = enumer.Current.Value;
                int id = enumer.Current.Key;
                if (m_instance.m_dic.ContainsKey(id))
                {
                    count--;
                    if (count <= 0)
                    {
                        onStoreOver();
                    }
                    continue;
                }
                else
                {
                    //External.ExternalManager.LoadAsset(External.EAssetType.TextAsset, path, (o) =>
                    //{
                    //    count--;
                    //    if (o != null)
                    //    {
                    //        var text = o as UnityEngine.TextAsset;
                    //        var data = MessagePack.MessagePackSerializer.Deserialize<BehaviorData>(text.bytes); //(BehaviorData)Json.Parse(typeof(BehaviorData), text.text);
                    //        m_instance.m_dic.Add(id, data);
                    //    }
                    //    if (count <= 0)
                    //    {
                    //        onStoreOver();
                    //    }
                    //});
                }
            }
        }
        public static BehaviorTree Get(int index)
        {
            if (m_instance.m_dic.ContainsKey(index))
            {
                var tree = ClassManager.Get<BehaviorTree>();
                tree.Initialize(m_instance.m_dic[index]);
                m_instance.m_usingTree.Add(tree);
                return tree;
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Behavior, "dont preload this behaviorTree=" + index);
                //if (Macro.isDebug)
                //{
                //    if (index > 0)
                //    {
                //        External.ExternalManager.LoadAsset(External.EAssetType.TextAsset, External.ExternalManager.GetSourcePath(index), (o) =>
                //        {
                //            if (o != null)
                //            {
                //                var text = o as UnityEngine.TextAsset;
                //                var data = MessagePack.MessagePackSerializer.Deserialize<BehaviorData>(text.bytes); //(BehaviorData)Json.Parse(typeof(BehaviorData), text.text);
                //                m_instance.m_dic.Add(index, data);
                //            }
                //            else
                //            {
                //                Debug.Log.i(Debug.ELogType.Behavior, " behaviorTree=" + index + "   dont exist in unity assets");
                //            }
                //        });
                //    }
                //    return Get(index);
                //}
                return null;
            }
        }
        public static void Free(BehaviorTree behavior)
        {
            ClassManager.Free(behavior);
            m_instance.m_usingTree.Remove(behavior);
        }
    }
}
