using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public class Utils
    {
        static Random _random = new Random((int)DateTime.Now.Ticks);

        public static int RandomNumber()
        {
            return _random.Next(1, 9);
        }

        public static double RandomtDoubleNumber()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// 日期转换为时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="isMilliseconds">是否毫秒</param>
        /// <returns></returns>
        public static long ConvertToTimeStamp(DateTime time, bool isMilliseconds = true)
        {
            System.DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(new System.DateTime(1970, 1, 1), TimeZoneInfo.Local);
            if (isMilliseconds)
            {
                return (int)(time - startTime).TotalMilliseconds;
            }
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 时间戳转换为日期
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="isMilliseconds">是否毫秒</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(long timeStamp, bool isMilliseconds = true)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new System.DateTime(1970, 1, 1), TimeZoneInfo.Local);
            if (isMilliseconds)
            {
                return dtStart.AddMilliseconds(timeStamp);
            }
            else
            {
                return dtStart.AddSeconds(timeStamp);
            }
        }
    }
}
