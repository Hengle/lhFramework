
using System.Collections;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public class Coroutine
    {
        private List<System.Collections.IEnumerator> m_enumerators;
        private List<System.Collections.IEnumerator> m_enumeratorsDelete;
        private List<System.Collections.IEnumerator> m_enumerSleep;
        private int capity;
        public Coroutine()
        {
            m_enumerators = new List<IEnumerator>(10);
            m_enumeratorsDelete = new List<IEnumerator>(10);
        }
        public Coroutine(int capity)
        {
            m_enumerators = new List<IEnumerator>(capity);
            m_enumeratorsDelete = new List<IEnumerator>(capity);
        }
        public void Update()
        {
            for (int i = 0; i < m_enumerators.Count; i++)
            {
                //if (m_enumerators[i].Current is WaitForSeconds)
                //{
                //    WaitForSeconds waitForSeconds = m_enumerators[i].Current as WaitForSeconds;
                //    waitForSeconds.time -= TimeManager.deltaTime;
                //    if (waitForSeconds.time > 0)
                //        continue;
                //}
                //--------------------------
                //others
                //--------------------------
                if (m_enumerators[i].MoveNext())
                {
                    continue;
                }
                m_enumeratorsDelete.Add(m_enumerators[i]);
            }
            for (int i = 0; i < m_enumeratorsDelete.Count; i++)
            {
                var del = m_enumeratorsDelete[i];
                m_enumerators.Remove(del);
            }
            m_enumeratorsDelete.Clear();
        }
        public void Start(IEnumerator enumerator)
        {
            if (enumerator == null) return;
#if UNITY_EDITOR
            if (m_enumerators.Contains(enumerator))
            {
                UnityEngine.Debug.LogError("Coroutine start error has this enumerator:" + enumerator);
                return;
            }
#endif
            m_enumerators.Add(enumerator);
        }
        public void Stop(IEnumerator enumerator)
        {
            if (enumerator == null) return;
#if UNITY_EDITOR
            if (!m_enumeratorsDelete.Contains(enumerator))
            {
                UnityEngine.Debug.LogError("Coroutine Stop error dont has this enumerator:" + enumerator);
                return;
            }
#endif
            m_enumeratorsDelete.Add(enumerator);
        }
        public void Sleep(IEnumerator enumerator)
        {
#if UNITY_EDITOR
            if (m_enumerSleep.Contains(enumerator))
            {
                UnityEngine.Debug.LogError("Coroutine sleep error has this enumerator:" + enumerator);
                return;
            }
#endif
            m_enumerSleep.Add(enumerator);
            m_enumeratorsDelete.Add(enumerator);
        }
        public void Awake(IEnumerator enumerator)
        {
#if UNITY_EDITOR
            if (!m_enumerSleep.Contains(enumerator))
            {
                UnityEngine.Debug.LogError("Coroutine Awake error m_enumerSleep dont has this enumerator:" + enumerator);
                return;
            }
            if (m_enumerators.Contains(enumerator))
            {
                UnityEngine.Debug.LogError("Coroutine Awake error m_enumerators has this enumerator:" + enumerator);
                return;
            }
#endif
            m_enumerSleep.Remove(enumerator);
            m_enumerators.Add(enumerator);
        }
        public void StopAll()
        {
            m_enumerators.Clear();
            m_enumeratorsDelete.Clear();
            m_enumerSleep.Clear();
        }
    }
}