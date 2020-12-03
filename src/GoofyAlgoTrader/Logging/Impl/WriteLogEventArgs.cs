using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Logging.Impl
{
    public class WriteLogEventArgs : EventArgs
    {
        public LogLevel Level { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }

        public DateTime Time { get; set; }

        public int ThreadID { get; set; }

        public bool IsThreadPoolThread { get; set; }

        public string ThreadName { get; set; }

        public int TaskID { get; set; }


        internal WriteLogEventArgs() { }

        [ThreadStatic]
        private static WriteLogEventArgs _Current;


        public static WriteLogEventArgs Current => _Current ??= new WriteLogEventArgs();

        /// <summary>
        /// 初始化为新日志
        /// </summary>
        public WriteLogEventArgs Set(LogLevel level)
        {
            Level = level;

            return this;
        }

        /// <summary>
        /// 初始化为新日志
        /// </summary>
        /// <param name="message">日志</param>
        /// <param name="exception">异常</param>
        public WriteLogEventArgs Set(string message, Exception exception)
        {
            Message = message;
            Exception = exception;

            Init();

            return this;
        }

        void Init()
        {
            Time = DateTime.Now;
            var thread = Thread.CurrentThread;
            ThreadID = thread.ManagedThreadId;
            IsThreadPoolThread = thread.IsThreadPoolThread;
            ThreadName = CurrentThreadName ?? thread.Name;

            var tid = Task.CurrentId;
            TaskID = tid != null ? tid.Value : -1;

        }

        public override string ToString()
        {
            if (Exception != null) Message = $"{Message} {GetExceptionMessage(Exception)}";

            var name = ThreadName;
            if (name.IsNullOrEmpty()) name = TaskID >= 0 ? TaskID + "" : "-";
            if (name.EqualIgnoreCase("Threadpool worker")) name = "P";
            if (name.EqualIgnoreCase("IO Threadpool worker")) name = "IO";

            return string.Format("{0:HH:mm:ss.fff} [{1}] {2} {3} {4} {5}", Time, Level.ToString(), ThreadID, IsThreadPoolThread ? 'P' : 'N', name, Message);
        }

        private string GetExceptionMessage(Exception ex)
        {
            var msg = ex + "";
            if (msg.IsNullOrEmpty()) return null;

            var ss = msg.Split(Environment.NewLine);
            var ns = ss.Where(e =>
            !e.StartsWith("---") &&
            !e.Contains("System.Runtime.ExceptionServices") &&
            !e.Contains("System.Runtime.CompilerServices"));

            msg = string.Join(Environment.NewLine, ns);

            return msg;
        }

        [ThreadStatic]
        private static string _threadName;

        /// <summary>
        /// 设置当前线程输出日志时的线程名
        /// </summary>
        public static string CurrentThreadName { get => _threadName; set => _threadName = value; }

    }
}
