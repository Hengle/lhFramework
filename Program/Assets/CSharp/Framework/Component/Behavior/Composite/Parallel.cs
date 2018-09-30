using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    [Task(ETaskType.Composite, "Parallel")]
    public class Parallel : CompositeTask
    {
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
