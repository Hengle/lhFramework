using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Infrastructure
{
    public enum ETimelineBindType
    {
        /// <summary>
        /// 脱离节点存在
        /// </summary>
        World,
        /// <summary>
        /// 节点本地
        /// </summary>
        Local,
        /// <summary>
        /// 原点
        /// </summary>
        Origin
    }
}
