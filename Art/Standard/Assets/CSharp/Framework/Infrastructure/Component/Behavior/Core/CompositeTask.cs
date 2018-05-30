using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public abstract class CompositeTask : ITask
    {
        public abstract EClassType classType { get; set; }
        public int[] tasks;
        int ITask.id { get; set; }
        int ITask.parentIndex { get; set; }
        int ITask.childIndex { get; set; }
        public abstract ETaskResult Execute(ETaskResult result, bool last);
        ETaskType ITask.taskType { get { return ETaskType.Composite; } set { } }
        public abstract void Ready();
        public abstract void End();
        public abstract void OnReset();
    }
}
