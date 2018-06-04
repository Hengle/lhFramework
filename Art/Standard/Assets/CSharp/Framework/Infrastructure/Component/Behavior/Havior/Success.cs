using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    [Task(ETaskType.Leaf, "Success")]
    public class Success : Havior
    {
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
