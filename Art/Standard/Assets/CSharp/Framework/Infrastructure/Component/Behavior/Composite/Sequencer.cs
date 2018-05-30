using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    [Task(ETaskType.Composite, "Sequencer")]
    public class Sequencer : CompositeTask
    {
        public override EClassType classType { get { return EClassType.Core_Sequence; } set { } }
        public override ETaskResult Execute(ETaskResult result, bool last)
        {
            if (last)
            {
                return result == ETaskResult.Failed ? ETaskResult.Failed : ETaskResult.Successed;
            }
            else
            {
                return result == ETaskResult.Failed ? ETaskResult.Failed : ETaskResult.Running;
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
