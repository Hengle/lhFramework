using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public interface ITask:IClass
    {
        /// <summary>
        ///task的id编号
        /// </summary>
        int id { get; set; }
        /// <summary>
        /// 父节点的索引
        /// </summary>
        int parentIndex { get; set; }
        /// <summary>
        /// 自己是第几个子节点
        /// </summary>
        int childIndex { get; set; }
        /// <summary>
        /// task类型
        /// </summary>
        ETaskType taskType { get; set; }
        /// <summary>
        /// task执行
        /// </summary>
        /// <returns></returns>
        ETaskResult Execute(ETaskResult childResult,bool last);
        /// <summary>
        /// task开始
        /// </summary>
        void Ready();
        /// <summary>
        /// task结束
        /// </summary>
        void End();
    }
}
