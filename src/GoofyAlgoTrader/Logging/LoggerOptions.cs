using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Logging
{
    public class LoggerOptions
    {
        /// <summary>
        /// 是否启用日志 默认true
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 日志等级 只输出大于等于该级别的日志，默认Info
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;


        /// <summary>
        /// 是否启用控制台日志 默认true
        /// </summary>
        public bool EnableConsoleLogger { get; set; } = true;
        /// <summary>
        /// 控制台日志 是否使用不同颜色显示，默认true
        /// </summary>
        public bool ConsoleLoggerUseColor { get; set; } = true;


        /// <summary>
        /// 文件日志 存储目录
        /// </summary>
        public string FileLoggerPath { get; set; } = "logs";

        /// <summary>
        /// 文件日志格式 默认{0:yyyy_MM_dd}.log
        /// </summary>
        public string FileLoggerFormat { get; set; } = "{0:yyyy_MM_dd}.log";

        /// <summary>
        /// 文件日志大小上限 超过上限后拆分新日志文件，默认10MB，0表示不限制大小
        /// </summary>
        public int FileLoggerMaxBytes { get; set; } = 10;

        /// <summary>
        /// 文件日志备份数 超过备份数后，最旧的文件将被删除，默认100，0表示不限制个数
        /// </summary>
        public int FileLoggerBackups { get; set; } = 100;
    }
}
