using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public class TaskAttribute:Attribute
    {
        public TaskAttribute(ETaskType taskType,string name)
        {
            this.taskType = taskType;
            this.name = name;
        }
        public ETaskType taskType { get; set; }
        public string name { get; set; }
    }
}
