using System.Collections;
using System.Collections.Generic;

namespace Framework.Infrastructure
{
    public interface IClass
    {
        EClassType classType { get; set; }
        /// <summary>
        /// 类对象重置
        /// </summary>
        void OnReset();
    }
}