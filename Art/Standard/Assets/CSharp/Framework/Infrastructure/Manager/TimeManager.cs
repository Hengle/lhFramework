using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Infrastructure
{
    public class TimeManager
    {
        public static int deltaTime { get; private set; }
        public static float fDeltaTime { get;private set; }
        public static int time { get; private set; }
        private static TimeManager m_instance;
        public static TimeManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new TimeManager();
        }
        TimeManager()
        {

        }
        ~TimeManager()
        {

        }
        public void Update()
        {
            time += deltaTime;
        }
        public void Dispose()
        {
            time = 0;
            deltaTime = 0;
            m_instance = null;
        }
        public static void ForceTime(int t)
        {
            time = t;
        }
        public void SetDeltaTime(int time)
        {
            deltaTime = time;
            fDeltaTime = time / 1000.0f;
        }
    }
}
