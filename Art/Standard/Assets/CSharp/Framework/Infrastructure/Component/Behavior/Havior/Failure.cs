using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    [Task(ETaskType.Leaf,"Failure")]
    public class Failure : Havior
    {
        public override ETaskResult Execute(ETaskResult result, bool last)
        {
            return ETaskResult.Failed;
        }
        public override void Parse(Dictionary<string, object> data)
        {

        }
        public override void SharedVariableChanged(string key, object value)
        {

        }
    }
}
