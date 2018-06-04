using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Components
{
    public struct BehaviorData
    {
        public struct TaskData
        {
            public int taskType;
            public int id;
            public string className;
            public Dictionary<string, object> param;
        }
        public int rootId;
        public Dictionary<int, int[]> composites;
        public Dictionary<int, int> decorators;
        public Dictionary<int, int> conditionals;
        public Dictionary<string, object> sharedVariables;
        public TaskData[] tasks;
    }
}
