using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
#pragma warning disable 0618
    [ExecuteInEditMode]
    public class EffectControl:MonoBehaviour,IEffect
    {
        public Vector3 stageOriginPosition;
        public EEffectDirection direction;
        EEffectType IEffect.effectType { get; set; }
        private int m_layer;
        int IEffect.index { get; set; }
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

        public ParticleSystem[] particleSystemArr;
        void IEffect.Create()
        {
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
            EffectManager.Free(this, time, OnFree);

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
            if (particleSystemArr!=null)
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
        void OnDisable()
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
            return gameObject.GetInstanceID();
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
