using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [Task(ETaskType.Leaf, "Success")]
    public class Success : Havior
    {
        public override EClassType classType { get { return EClassType.Core_Success; } set { } }
        public override ETaskResult Execute(ETaskResult result, bool last)
        {
            return ETaskResult.Successed;
        }
        public override void Parse(Dictionary<string, object> data)
        {

        }
        public override void SharedVariableChanged(string key, object value)
        {

        }
    }
}
