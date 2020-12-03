using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Logging.Impl
{
    public class ConsoleLogger : Logger
    {
        public bool UseColor { get; set; } = Config.LoggerOptions.ConsoleLoggerUseColor;

        protected override void WriteLog(LogLevel level, string message, Exception ex)
        {
            var e = WriteLogEventArgs.Current.Set(level).Set(message, ex);

            if (!UseColor)
            {
                ConsoleWriteLog(e);
                return;
            }

            lock (this)
            {
                var cc = Console.ForegroundColor;
                switch (level)
                {
                    case LogLevel.Debug:
                        cc = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        cc = ConsoleColor.Red;
                        break;
                    default:
                        cc = GetColor(e.ThreadID);
                        break;
                }

                var old = Console.ForegroundColor;
                Console.ForegroundColor = cc;
                ConsoleWriteLog(e);
                Console.ForegroundColor = old;
            }
        }

        private void ConsoleWriteLog(WriteLogEventArgs e)
        {
            var msg = e.ToString();
            Console.WriteLine(msg);
        }

        static readonly ConcurrentDictionary<int, ConsoleColor> dic = new ConcurrentDictionary<int, ConsoleColor>();
        static readonly ConsoleColor[] colors = new ConsoleColor[] {
            ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.Yellow,
            ConsoleColor.DarkGreen, ConsoleColor.DarkCyan, ConsoleColor.DarkMagenta, ConsoleColor.DarkRed, ConsoleColor.DarkYellow };

        private ConsoleColor GetColor(int threadid)
        {
            if (threadid == 1) return ConsoleColor.Gray;

            return dic.GetOrAdd(threadid, k => colors[dic.Count % colors.Length]);
        }

        public override string ToString()
        {
            return string.Format("{0} UseColor={1}", GetType().Name, UseColor);
        }
    }
}
