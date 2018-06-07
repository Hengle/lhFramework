using System;
using System.Collections.Generic;

namespace lhFramework.Debug
{
    public class Log
    {
        public static void i(string value)
        {
            i(ELogType.Info, value);
        }
        public static void i(ELogType type,string value)
        {

        }
        public static void f(string value, int mark)
        {
            f(ELogType.Info, value, mark);
        }
        public static void f(ELogType type,string value,int mark)
        {

        }
    }
}
