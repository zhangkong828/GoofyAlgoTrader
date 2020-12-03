using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Logging
{
    public interface ILogger
    {
        void Trace(string message);
        void Trace(string format, params object[] args);
        void Trace(string message, Exception ex);

        void Debug(string message);
        void Debug(string format, params object[] args);
        void Debug(string message, Exception ex);

        void Info(string message);
        void Info(string format, params object[] args);
        void Info(string message, Exception ex);

        void Error(string message);
        void Error(string format, params object[] args);
        void Error(string message, Exception ex);

        void Fatal(string message);
        void Fatal(string format, params object[] args);
        void Fatal(string message, Exception ex);

        void Write(LogLevel level, Exception ex, string format, params object[] args);

        void Release();
    }

    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Error = 3,
        Fatal = 4
    }
}
