using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Logging
{
    public abstract class Logger : ILogger
    {
        /// <summary>
        /// 是否启用日志
        /// </summary>
        public virtual bool Enable { get; set; } = Config.LoggerOptions.Enable;

        /// <summary>
        /// 日志等级，只输出大于等于该级别的日志
        /// </summary>
        public virtual LogLevel Level { get; set; } = Config.LoggerOptions.LogLevel;


        public virtual void Write(LogLevel level, Exception ex, string format, params object[] args)
        {
            if (Enable && level >= Level)
            {
                WriteLog(level, string.Format(format, args), ex);
            }
        }

        protected abstract void WriteLog(LogLevel level, string message, Exception ex);


        public void Trace(string message) => Write(LogLevel.Trace, null, message);
        public void Trace(string format, params object[] args) => Write(LogLevel.Trace, null, format, args);
        public void Trace(string message, Exception ex) => Write(LogLevel.Trace, ex, message);

        public void Debug(string message) => Write(LogLevel.Debug, null, message);
        public void Debug(string format, params object[] args) => Write(LogLevel.Debug, null, format, args);
        public void Debug(string message, Exception ex) => Write(LogLevel.Debug, ex, message);

        public void Info(string message) => Write(LogLevel.Info, null, message);
        public void Info(string format, params object[] args) => Write(LogLevel.Info, null, format, args);
        public void Info(string message, Exception ex) => Write(LogLevel.Info, ex, message);

        public void Error(string message) => Write(LogLevel.Error, null, message);
        public void Error(string format, params object[] args) => Write(LogLevel.Error, null, format, args);
        public void Error(string message, Exception ex) => Write(LogLevel.Error, ex, message);

        public void Fatal(string message) => Write(LogLevel.Fatal, null, message);
        public void Fatal(string format, params object[] args) => Write(LogLevel.Fatal, null, format, args);
        public void Fatal(string message, Exception ex) => Write(LogLevel.Fatal, ex, message);


        /// <summary>
        /// 输出日志头，包含所有环境信息
        /// </summary>
        protected static string GetHead()
        {
            return null;
        }

        public virtual void Release() { }
    }
}
