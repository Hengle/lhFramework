using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
#pragma warning disable 0108
    [ExecuteInEditMode]
    public class SubEffectTarget:MonoBehaviour
    {
        public float subEffectHeightOfHide=500;
        public ParticleSystem particleSystem;
        private ParticleSystem.Particle[] m_particleArr = new ParticleSystem.Particle[12];
        private SubParticleTrace[] m_traceList = new SubParticleTrace[12];
        public ParticleSystem[] subParticleSystemArr;
        public int maxParticles;
        private int m_layer;
        
        void OnEnable()
        {
#if UNITY_EDITOR
            subParticleSystemArr = gameObject.GetComponentsInChildren<ParticleSystem>();
            particleSystem = GetComponent<ParticleSystem>();
            maxParticles = particleSystem.main.maxParticles;
#endif
            if (Application.isPlaying)
            {
                for (int i = 0; i < m_particleArr.Length; i++)
                {
                    m_particleArr[i].startLifetime = 999999;
                    m_particleArr[i].remainingLifetime = 999999;
                    m_particleArr[i].startSize = 1;
                    m_particleArr[i].position = new Vector3(0, subEffectHeightOfHide, 0);
                }
                particleSystem.Emit(m_particleArr.Length);
                particleSystem.SetParticles(m_particleArr, m_particleArr.Length);
            }
        }
#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
            {
                maxParticles = particleSystem.main.maxParticles;
            }
        }
#endif
        void OnDisable()
        {
            for (int i = 0; i < m_traceList.Length; i++)
            {
                ClassManager.Free(m_traceList[i]);
                m_traceList[i] = null;
            }
            particleSystem.SetParticles(null, 0);
        }
        public bool OnUpdate()
        {
            if (enabled)
            {
                bool has = false;
                for (int i = 0; i < m_traceList.Length; i++)
                {
                    var trace = m_traceList[i];
                    if (trace == null) continue;
                    has = true;
                    trace.time -=Mathf.RoundToInt( Time.deltaTime*1000);
                    if (trace.bindType == EEffectBindType.Local)
                    {
                        var inverse = trace.transform == null ? transform.position : transform.InverseTransformPoint(trace.transform.position);
                        if (inverse != m_particleArr[trace.particleIndex].position)
                        {
                            int numParticle = particleSystem.GetParticles(m_particleArr);
                            m_particleArr[trace.particleIndex].position = inverse + transform.localPosition + trace.offset;
                            m_particleArr[trace.particleIndex].rotation3D = trace.transform.eulerAngles;
                            particleSystem.SetParticles(m_particleArr, numParticle);
                        }
                    }
                    if (trace.time <= 0)
                    {
                        int numParticle = particleSystem.GetParticles(m_particleArr);
                        m_particleArr[trace.particleIndex].position = new Vector3(0, subEffectHeightOfHide, 0);
                        particleSystem.SetParticles(m_particleArr, numParticle);
                        ClassManager.Free(trace);
                        m_traceList[i] = null;
                    }
                }
                return has;
            }
            return false;
        }
        public void OnPause()
        {
            if (enabled)
                particleSystem.Pause();
        }
        public void OnResume()
        {
            if (enabled)
            {
                if (particleSystem.IsAlive())
                {
                    particleSystem.Play();
                }
            }

        }
        public void Bind(int index, Transform trans, Vector3 offset, EEffectBindType bindType, int time)
        {
            index = index - 1;
            for (int i = 0; i < m_traceList.Length; i++)
            {
                if (m_traceList[i] == null) continue;
                if (m_traceList[i].particleIndex == index)
                {
                    m_traceList[i].time = time;
                    m_traceList[i].offset = offset;
                    m_traceList[i].bindType = bindType;
                    m_traceList[i].transform = trans;

                    int n = particleSystem.GetParticles(m_particleArr);
                    var inver = trans == null ? transform.position: transform.InverseTransformPoint(trans.position);
                    m_particleArr[index].position = inver + transform.localPosition + offset;
                    m_particleArr[index].rotation3D = trans.eulerAngles;
                    particleSystem.SetParticles(m_particleArr, n);
                    return;
                }
            }
            var trace = ClassManager.Get<SubParticleTrace>(EClassType.Core_SubParticleTrace);
            trace.offset = offset;
            trace.particleIndex = index;
            trace.time = time;
            trace.transform = trans;
            trace.bindType = bindType;
            for (int i = 0; i < m_traceList.Length; i++)
            {
                if (m_traceList[i]==null)
                {
                    m_traceList[i]=trace;
                    break;
                }
            }

            int numParticle = particleSystem.GetParticles(m_particleArr);
            var inverse = trans == null ? transform.position:transform.InverseTransformPoint(trans.position);
            m_particleArr[index].position = inverse + transform.localPosition + offset;
            m_particleArr[index].rotation3D = trans.eulerAngles;
            particleSystem.SetParticles(m_particleArr, numParticle);
        }
        public void UnBind(int index)
        {
            index = index - 1;
            for (int i = 0; i < m_traceList.Length; i++)
            {
                if (m_traceList[i]!=null && m_traceList[i].particleIndex == index)
                {
                    m_traceList[i].time = 0;
                    return;
                }
            }
        }
        public void SetLayer(int layer)
        {
            if (subParticleSystemArr != null && subParticleSystemArr.Length > 0)
            {
                m_layer = subParticleSystemArr[0].gameObject.layer;
                for (int i = 0; i < subParticleSystemArr.Length; i++)
                {
                    subParticleSystemArr[i].gameObject.layer = layer;
                }
            }
        }
        public void ResetLayer()
        {
            if (subParticleSystemArr != null && subParticleSystemArr.Length > 0)
            {
                for (int i = 0; i < subParticleSystemArr.Length; i++)
                {
                    subParticleSystemArr[i].gameObject.layer = m_layer;
                }
            }
        }
    }
}
