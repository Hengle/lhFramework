using System;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public enum EEffectType
    {
        General,
        Sub,
        AddCommander,
    }
    public enum EEffectBindType
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
