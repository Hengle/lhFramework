using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [Task(ETaskType.Composite, "Selector")]
    public class Selector : CompositeTask
    {
        public override EClassType classType { get { return EClassType.Core_Selector; } set { } }
        public override ETaskResult Execute(ETaskResult result,bool last)
        {
            if (last)
            {
                return result == ETaskResult.Successed ? ETaskResult.Successed : ETaskResult.Failed;
            }
            else
            {
                return result == ETaskResult.Successed ? ETaskResult.Successed : ETaskResult.Running;
            }
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
