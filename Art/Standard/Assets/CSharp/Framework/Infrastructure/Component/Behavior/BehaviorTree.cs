using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
#pragma warning disable 0414
    public class BehaviorTree : IClass
    {
        public int mark=0;
        EClassType IClass.classType { get { return EClassType.Core_BehaviorTree; } set { } }
        private int[] m_taskKeys;
        private ITask[] m_taskValues;
        private ITask m_runningTask;
        private List<int> m_nodeTree = new List<int>();
        private int[] m_nodeTreeArr;
        private ITask m_firstExeNode;
        private int m_logCount = 99999;
        private int m_frameCount;
        public void Initialize(BehaviorData data)
        {
            m_taskKeys = new int[data.tasks.Length+1];
            m_taskValues = new ITask[data.tasks.Length + 1];
            for (int i = 0; i < data.tasks.Length; i++)
            {
                var taskData = data.tasks[i];
                var cls = ClassManager.Get(taskData.className) as ITask;
                cls.id = taskData.id;
                cls.taskType = (ETaskType)taskData.taskType;
                if (cls.taskType == ETaskType.Composite)
                {
                    ((CompositeTask)cls).tasks = data.composites[taskData.id];
                }
                else if (cls.taskType == ETaskType.Decorator)
                {
                    ((DecoratorTask)cls).childTask = data.decorators[taskData.id];
                    ((DecoratorTask)cls).Parse(taskData.param);
                }
                else
                    ((LeafTask)cls).Parse(taskData.param);
                m_taskKeys[i+1] = taskData.id;
                m_taskValues[i+1] = cls;
            }
            AddCompositeNode(data.rootId,-1, data.composites[data.rootId]);
            var sharedEnmer = data.sharedVariables.GetEnumerator();
            while (sharedEnmer.MoveNext())
            {
                SetVariable(sharedEnmer.Current.Key, sharedEnmer.Current.Value);
            }
            m_nodeTreeArr = m_nodeTree.ToArray();
            m_nodeTree.Clear();
        }
        void IClass.OnReset()
        {
            m_nodeTreeArr = null;
            m_firstExeNode = null;
            m_runningTask = null;
            m_frameCount = 0;
            mark = 0;
            for (int i = 0; i < m_taskValues.Length; i++)
            {
                ClassManager.Free(m_taskValues[i]);
            }
            m_frameCount = 0;
            m_logCount = 9999;
        }
        public void Update()
        {
            m_frameCount++;
            if (m_runningTask==null)
            {
                ExecuteLeaf(m_firstExeNode);
            }
            else{
                ETaskResult result = m_runningTask.Execute(ETaskResult.Successed, false);
                TaskStatus(m_runningTask.id, result);
                if (result!=ETaskResult.Running)
                {
                    int compositeId = m_nodeTreeArr[m_runningTask.parentIndex];
                    //int childLength = m_nodeTreeArr[m_runningTask.parentIndex + 1];
                    ITask parentTask = m_taskValues[compositeId];
                    if (parentTask is CompositeTask)
                    {
                        ToExecuteComposite(m_runningTask.parentIndex, m_runningTask.childIndex, parentTask, result);
                    }
                    else if (parentTask is DecoratorTask)
                    {
                        ToExecuteDecorator(m_runningTask.parentIndex, parentTask, result);
                    }
                    else
                    {
                        Debug.Log.i(Debug.ELogType.Behavior, "leaf parent must Composite or Decorator");
                    }
                    m_runningTask.End();
                    m_runningTask = null;
                }
            }

        }
        public void SetVariable(string key, object value)
        {
            for (int i = 0; i < m_taskValues.Length; i++)
            {
                if (m_taskValues[i] is LeafTask)
                {
                    ((LeafTask)m_taskValues[i]).SharedVariableChanged(key, value);
                }
            }
        }
        public void SetLogCount(int count)
        {
            m_logCount = count;
        }
        public void OnDrawGizmos()
        {
            for (int i = 0; i < m_taskValues.Length; i++)
            {
                if (m_taskValues[i] is LeafTask)
                {
                    ((LeafTask)m_taskValues[i]).OnDrawGizmos();
                }
            }
        }
        public void OnDrawGizmosSelected()
        {
            for (int i = 0; i < m_taskValues.Length; i++)
            {
                if (m_taskValues[i] is LeafTask)
                {
                    ((LeafTask)m_taskValues[i]).OnDrawGizmosSelected();
                }
            }
        }
        void AddCompositeNode(int taskId,int parentNodeId,int[] childs)
        {
            m_nodeTree.Add(taskId);
            m_taskValues[taskId].parentIndex = parentNodeId;
            int nodeId = m_nodeTree.Count - 1;
            m_nodeTree.Add(childs.Length);
            for (int i = 0; i < childs.Length; i++)
            {
                m_nodeTree.Add(childs[i]);
            }
            for (int i = 0; i < childs.Length; i++)
            {
                int c = childs[i];
                ITask task = m_taskValues[c];
                task.parentIndex = nodeId;
                task.childIndex = i;
                if (task is CompositeTask)
                {
                    AddCompositeNode(c, nodeId,((CompositeTask)task).tasks);
                }
                else if (task is DecoratorTask)
                {
                    AddDecoratorNode(c, nodeId,((DecoratorTask)task).childTask);
                }
                else
                {
                    if (m_firstExeNode==null)
                    {
                        m_firstExeNode = task;
                    }
                }
            }

        }
        void AddDecoratorNode(int taskId,int parentNodeId,int child)
        {
            m_nodeTree.Add(taskId);
            int nodeId = m_nodeTree.Count - 1;
            ITask task = m_taskValues[child];
            task.parentIndex = nodeId;
            task.childIndex = 0;
            if (task is CompositeTask)
            {
                AddCompositeNode(child, nodeId, ((CompositeTask)task).tasks);
            }
            else if (task is DecoratorTask)
            {
                AddDecoratorNode(child, nodeId, ((DecoratorTask)task).childTask);
            }
            else
            {
                if (m_firstExeNode == null)
                {
                    m_firstExeNode = task;
                }
            }
        }
        void ExecuteLeaf(ITask task)
        {
            task.Ready();
            ETaskResult result = task.Execute(ETaskResult.Successed,false);
            TaskStatus(task.id, result);
            if (result == ETaskResult.Running)
            {
                m_runningTask = task;
            }
            else
            {
                int compositeId = m_nodeTreeArr[task.parentIndex];
                //int childLength = m_nodeTreeArr[task.parentIndex + 1];
                ITask parentTask = m_taskValues[compositeId];
                if (parentTask is CompositeTask)
                {
                    ToExecuteComposite(task.parentIndex, task.childIndex, parentTask, result);
                }
                else if (parentTask is DecoratorTask)
                {
                    ToExecuteDecorator(task.parentIndex, parentTask, result);
                }
                else
                {
                    Debug.Log.i(Debug.ELogType.Behavior, "leaf parent must Composite or Decorator");
                }
                task.End();
            }
        }
        void ToExecuteComposite(int nodeId, int indexOfChild, ITask task, ETaskResult r)
        {
            int childLength = m_nodeTreeArr[nodeId + 1];
            bool last = indexOfChild == childLength - 1;
            task.Ready();
            ETaskResult result = task.Execute(r, last);
            if (result==ETaskResult.Running)
            {
                int childNodeId = nodeId + 1 + indexOfChild + 2;
                int childTaskId = m_nodeTreeArr[childNodeId];
                ITask childTask = m_taskValues[childTaskId];
                if (childTask.taskType==ETaskType.Composite)
                {
                    ExecuteCompositeTo(childNodeId, childTask);
                }
                else if (childTask.taskType==ETaskType.Decorator)
                {
                    ExecuteDecoratorTo(childNodeId, childTask);
                }
                else
                {
                    ExecuteLeaf(childTask);
                }
            }
            else
            {
                TaskStatus(task.id, result);
                if (task.parentIndex>=0)
                {
                    int compositeId = m_nodeTreeArr[task.parentIndex];
                    ITask parentTask = m_taskValues[compositeId];
                    ToExecuteComposite(task.parentIndex, task.childIndex, parentTask, result);
                }
                task.End();
            }
        }
        void ExecuteCompositeTo(int nodeId, ITask task)
        {
            task.Ready();
            if (task.taskType==ETaskType.Composite)
            {
                CompositeTask comTask = (CompositeTask)task;
                int childCompositeId=comTask.tasks[0];
                ITask childTask = m_taskValues[childCompositeId];
                int childNodeId = childTask.parentIndex+1+1;
                if (childTask.taskType == ETaskType.Composite)
                {
                    ExecuteCompositeTo(childNodeId, childTask);
                }
                else if (childTask.taskType == ETaskType.Decorator)
                {
                    ExecuteDecoratorTo(childNodeId, childTask);
                }
                else {
                    ExecuteLeaf(childTask);
                }
            }
            else if (task.taskType==ETaskType.Decorator)
            {
                ExecuteDecoratorTo(nodeId + 1 + 1, task);
            }
            else
            {
                ExecuteLeaf(task);
            }
            task.End();
        }
        void ToExecuteDecorator(int nodeId, ITask task, ETaskResult r)
        {
            task.Ready();
            ETaskResult result = task.Execute(r, true);
            TaskStatus(task.id, result);
            if (result == ETaskResult.Running)
            {

            }
            else
            {
                if (task.parentIndex >= 0)
                {
                    int compositeId = m_nodeTreeArr[task.parentIndex];
                    ITask parentTask = m_taskValues[compositeId];
                    if (parentTask is CompositeTask)
                    {
                        ToExecuteComposite(task.parentIndex, task.childIndex, parentTask, result);
                    }
                    else if (parentTask is DecoratorTask)
                    {
                        ToExecuteDecorator(task.parentIndex, parentTask, result);
                    }
                }
                task.End();
            }
        }
        void ExecuteDecoratorTo(int nodeId,ITask t)
        {
            t.Ready();
            //int decoratorId = m_nodeTreeArr[nodeId];
            DecoratorTask task =t as DecoratorTask;
            ITask childTask = m_taskValues[task.childTask];
            if (childTask is CompositeTask)
            {
                ExecuteCompositeTo(task.childTask, childTask);
            }
            else if (childTask is DecoratorTask)
            {
                ExecuteDecoratorTo(task.childTask, childTask);
            }
            else
            {
                ExecuteLeaf(childTask);
            }
            t.End();
        }
        void TaskStatus(int id, ETaskResult result)
        {
#if UNITY_EDITOR
            if (Macro.log)
            {
                if (m_logCount > 0)
                {
                    m_logCount--;
                    string color = "";
                    if (result == ETaskResult.Successed)
                    {
                        color = "#65DB0BFF";
                    }
                    else if (result == ETaskResult.Running)
                    {
                        color = "#FFFFFFFF";
                    }
                    else
                    {
                        color = "#FF2D00FF";
                    }
                    Debug.Log.f(Debug.ELogType.Behavior, "<color=#727272FF>[" + m_frameCount + "]</color>" + "[" + mark + "]==> " + id + " --- " + m_taskValues[id].GetType().Name + "   <color=" + color + ">  " + result.ToString() + "</color>",mark);
                }
            }
#endif
        }
    }
}
