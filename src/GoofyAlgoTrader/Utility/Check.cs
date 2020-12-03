using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public static class Check
    {
        /// <summary>
        /// 校验参数不能为null
        /// </summary>
        public static void NotNull<T>(T argument, string argumentName = null) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName + " 不能为null.");
        }

        /// <summary>
        /// 校验参数不能为null或empty
        /// </summary>
        public static void NotNullOrEmpty(string argument, string argumentName = null)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentNullException(argument, argumentName + " 不能为null或empty.");
        }

        /// <summary>
        /// 校验参数不能小于等于0
        /// </summary>
        public static void Positive(int number, string argumentName = null)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " 不能小于等于0.");
        }

        /// <summary>
        /// 校验参数不能小于等于0
        /// </summary>
        public static void Positive(long number, string argumentName)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " 不能小于等于0.");
        }

        /// <summary>
        /// 校验参数不能小于0
        /// </summary>
        public static void Nonnegative(int number, string argumentName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " 不能小于0.");
        }

        /// <summary>
        /// 校验参数不能小于0
        /// </summary>
        public static void Nonnegative(long number, string argumentName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " 不能小于0.");
        }


    }
}
