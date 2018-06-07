using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
#pragma warning disable 0618
    [ExecuteInEditMode]
    public class EffectControl:MonoBehaviour,IEffect
    {
        public Vector3 stageOriginPosition;
        public EEffectDirection direction;
        private int m_layer;
        int IEffect.index { get; set; }
        EEffectGroup IEffect.group { get; set; }
        GameObject IEffect.gameObject { get { return m_gameObject; } set { } }
        Transform IEffect.transform { get { return m_transform; } set { } }

        private Transform m_transform;
        private GameObject m_gameObject;
        public ParticleSystem[] particleSystemArr;
        void IEffect.Create()
        {
            m_transform = transform;
            m_gameObject = gameObject;
            if (particleSystemArr==null)
            {
                particleSystemArr = gameObject.GetComponentsInChildren<ParticleSystem>();
            }
        }
        public void OnPause()
        {
            if (enabled && particleSystemArr != null)
            {
                for (int i = 0; i < particleSystemArr.Length; i++)
                {
                    particleSystemArr[i].Pause();
                }
            }
        }
        public void OnResume()
        {
            if (enabled && particleSystemArr!=null)
            {
                for (int i = 0; i < particleSystemArr.Length; i++)
                {
                    if (particleSystemArr[0].IsAlive())
                    {
                        particleSystemArr[i].Play();
                    }
                }
            }
        }
        void IEffect.OnUpdate()
        {
        }
        void IEffect.Bind(int index, Transform trans, Vector3 offset,EEffectBindType bindType, int time,Vector3 normal)
        {
            if (bindType==EEffectBindType.Local)
            {
                this.transform.SetParent(trans);
                this.transform.localPosition=Vector3.zero+offset;
            }
            else if (bindType ==EEffectBindType.Origin)
            {
                this.transform.SetParent(null);
                this.transform.position = stageOriginPosition+ offset;
            }
            else
            {
                if (trans!=null)
                {
                    this.transform.SetParent(null);
                    this.transform.position = trans.position + offset;
                }
            }
            if (direction==EEffectDirection.forward)
            {
                this.transform.rotation=Quaternion.LookRotation(normal, trans.up);
            }
            else if (direction==EEffectDirection.backward)
            {
                this.transform.rotation = Quaternion.LookRotation(normal*-1, trans.up);
            }
            EffectManager.Free(this, time,((IEffect)this).group, OnFree);

        }
        void IEffect.UnBind(int id)
        {
            EffectManager.Free(this, -1);
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                particleSystemArr = gameObject.GetComponentsInChildren<ParticleSystem>();
            }
#endif
            if (Application.isPlaying)
            {
                if (particleSystemArr != null)
                {
                    for (int i = 0; i < particleSystemArr.Length; i++)
                    {
                        var item = particleSystemArr[i];
                        ParticleSystem.EmissionModule psemit = item.emission;
                        psemit.enabled = true;
                        item.Simulate(0.0f, true, true);
                        item.Clear();
                        item.Play();
                        item.enableEmission = true;
                    }
                }
            }

        }
        void OnDisable()
        {
            if (Application.isPlaying)
            {
                if (particleSystemArr != null)
                {
                    for (int i = 0; i < particleSystemArr.Length; i++)
                    {
                        var item = particleSystemArr[i];
                        ParticleSystem.EmissionModule psemit = item.emission;
                        psemit.enabled = false;
                        item.time = 0;
                        item.Stop();
                    }
                }
            }
        }
        void IEffect.ResetLayer()
        {
            if (particleSystemArr != null && particleSystemArr.Length > 0)
            {
                for (int i = 0; i < particleSystemArr.Length; i++)
                {
                    particleSystemArr[i].gameObject.layer = m_layer;
                }
            }
        }
        void IEffect.SetLayer(int layer)
        {
            if (particleSystemArr!=null && particleSystemArr.Length>0)
            {
                m_layer = particleSystemArr[0].gameObject.layer;
                for (int i = 0; i < particleSystemArr.Length; i++)
                {
                    particleSystemArr[i].gameObject.layer = layer;
                }
            }
        }
        int IEffect.InstanceID()
        {
            return ((IEffect)this).gameObject.GetInstanceID();
        }
        void OnFree()
        {
            for (int i = 0; i < particleSystemArr.Length; i++)
            {
                particleSystemArr[i].gameObject.layer = m_layer;
            }
        }
    }
}
