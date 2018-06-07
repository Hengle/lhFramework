using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    public class Interval:DecoratorTask
    {
        private int m_time;
        private int m_curTime;
        public override ETaskResult Execute(ETaskResult result,bool last)
        {
            m_curTime -= TimeManager.deltaTime;
            if (m_curTime <= 0)
            {
                m_curTime = m_time;
                return ETaskResult.Successed;
            }
            else
                return ETaskResult.Failed;
        }
        public override void Parse(Dictionary<string, object> data)
        {
            if (data.ContainsKey("time"))
            {
                m_time = Convert.ToInt32(data["time"]);
            }
        }
        public override void SharedVariableChanged(string key, object value)
        {

        }
        public override void Ready()
        {

        }
        public override void End()
        {

        }
        public override void OnReset()
        {
        }
    }
}
