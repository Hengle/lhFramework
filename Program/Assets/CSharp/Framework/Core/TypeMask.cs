using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Core
{
    public class TypeMask
    {
        /// <summary>
        /// 掩码值
        /// </summary>
        public int mask { get; private set; }
        /// <summary>
        /// 是否与另外一个数有相同的1位码
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(int other)
        {
            return (mask & other) != 0;
        }
        /// <summary>
        /// 合并2个数的1位码
        /// @param other
        /// </summary>
        /// <param name="other"></param>
        public int Add(int other)
        {
            mask |= other;
            return mask;
        }
        /// <summary>
        /// 过滤与另外一个数的相同位的1位码
        /// </summary>
        /// <param name="other"></param>
        public int Remove(int other)
        {
            mask &= ~other;
            return mask;
        }
        /// <summary>
        /// 重置，清空所有1位码
        /// </summary>
        public int Reset()
        {
            return this.mask = 0;
        }
        public static bool Contain(int mask, int other)
        {
            return (mask & other) != 0;
        }
    }
}
