using System;
using UnityEngine;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [ExecuteInEditMode]
    public class SubEffectControl:MonoBehaviour,IEffect
    {
        int IEffect.index { get; set; }
        EEffectType IEffect.effectType { get; set; }
        GameObject IEffect.gameObject
        {
            get
            {
                return base.gameObject;
            }
            set { value = base.gameObject; }
        }
        Transform IEffect.transform
        {
            get
            {
                return base.transform;
            }
            set { value = base.transform; }
        }
        public SubEffectTarget[] targetList;
        void IEffect.Create()
        {
            if (targetList==null)
            {
                targetList = new SubEffectTarget[transform.childCount];
                int i = 0;
                foreach (Transform trans in transform)
                {
                    if (trans == transform) continue;
                    var target = trans.GetComponent<SubEffectTarget>();
                    if (target == null)
                    {
                        target = trans.gameObject.AddComponent<SubEffectTarget>();
                    }
                    targetList[i]=target;
                    i++;
                }
            }
        }
#if UNITY_EDITOR
        void OnEnable()
        {
            if (Application.isPlaying) return;
            targetList = new SubEffectTarget[transform.childCount];
            int i = 0;
            foreach (Transform trans in transform)
            {
                if (trans == transform) continue;
                var target = trans.GetComponent<SubEffectTarget>();
                if (target == null)
                {
                    target = trans.gameObject.AddComponent<SubEffectTarget>();
                }
                targetList[i] = target;
                i++;
            }
        }
#endif
        public void OnPause()
        {
            if (enabled)
            {
                for (int i = 0; i < targetList.Length; i++)
                {
                    targetList[i].OnPause();
                }
            }
        }
        public void OnResume()
        {
            if (enabled)
            {
                for (int i = 0; i < targetList.Length; i++)
                {
                    targetList[i].OnResume();
                }
            }
        }
        void IEffect.OnUpdate()
        {
            if (enabled)
            {
                bool has = false;
                for (int i = 0; i < targetList.Length; i++)
                {
                   bool h=targetList[i].OnUpdate();
                    if (h)
                    {
                        has = true;
                    }
                }
                if (!has)
                {
                    EffectManager.Free(this);
                }
            }
        }
        void IEffect.Bind(int index,Transform trans, Vector3 offset,EEffectBindType bindType, int time, Vector3 dir)
        {
            for (int i = 0; i < targetList.Length; i++)
            {
                targetList[i].Bind(index, trans, offset, bindType, time);
            }
        }
        void IEffect.UnBind(int index)
        {
            for (int i = 0; i < targetList.Length; i++)
            {
                targetList[i].UnBind(index);
            }
        }
        int IEffect.InstanceID()
        {
            return gameObject.GetInstanceID();
        }
        void IEffect.ResetLayer()
        {
            for (int i = 0; i < targetList.Length; i++)
            {
                targetList[i].ResetLayer();
            }
        }
        void IEffect.SetLayer(int layer)
        {
            for (int i = 0; i < targetList.Length; i++)
            {
                targetList[i].SetLayer(layer);
            }
        }
        //#if UNITY_EDITOR

        //        void OnDrawGizmos()
        //        {
        //            if (Macro.gizmos)
        //            {
        //                Gizmos.color = Color.blue;
        //                for (int i = 0; i < m_traceList.Count; i++)
        //                {
        //                    Gizmos.DrawLine(transform.position, m_traceList[i].transform.position);
        //                    Gizmos.DrawWireSphere(m_traceList[i].transform.position, 0.2f);
        //                }
        //            }
        //        }
        //#endif
    }
}
