using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [Task(ETaskType.Composite, "Parallel")]
    public class Parallel : CompositeTask
    {
        public override EClassType classType { get { return EClassType.Core_Parallel; } set { } }
        public override ETaskResult Execute(ETaskResult result, bool last)
        {
            return last ? ETaskResult.Successed : ETaskResult.Running;
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
