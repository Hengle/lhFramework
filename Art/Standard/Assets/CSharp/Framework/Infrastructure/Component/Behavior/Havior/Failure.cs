using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [Task(ETaskType.Leaf,"Failure")]
    public class Failure : Havior
    {
        public override EClassType classType { get { return EClassType.Core_Failure; } set { } }
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
