using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    using Managers;
    public abstract class LeafTask : ITask
    {
        int ITask.id { get; set; }
        int ITask.parentIndex { get; set; }
        int ITask.childIndex { get; set; }
        public abstract ETaskResult Execute(ETaskResult result, bool last);
        ETaskType ITask.taskType { get { return ETaskType.Leaf; } set { } }
        public abstract void Parse(Dictionary<string, object> data);
        public abstract void SharedVariableChanged(string key, object value);
        public abstract void Ready();
        public abstract void End();
        public abstract void OnReset();
        public virtual void OnDrawGizmos() { }
        public virtual void OnDrawGizmosSelected() { }


    }
}
