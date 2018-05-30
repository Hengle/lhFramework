using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public class Inverter : DecoratorTask
    {
        public override EClassType classType { get { return EClassType.Core_Inverter; } set { } }
        public override ETaskResult Execute(ETaskResult result, bool last)
        {
            return result == ETaskResult.Successed ? ETaskResult.Failed: ETaskResult.Successed;
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
        public override void Parse(Dictionary<string, object> data)
        {

        }
        public override void SharedVariableChanged(string key, object value)
        {

        }
    }
}
