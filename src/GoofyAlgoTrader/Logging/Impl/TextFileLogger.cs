using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GoofyAlgoTrader.Logging.Impl
{
    public class TextFileLogger : Logger, IDisposable
    {
        /// <summary>
        /// 日志目录
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// 日志文件格式。默认{0:yyyy_MM_dd}.log
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// 日志文件上限。超过上限后拆分新日志文件，默认10MB，0表示不限制大小
        /// </summary>
        public int MaxBytes { get; set; }

        /// <summary>
        /// 日志文件备份。超过备份数后，最旧的文件将被删除，默认100，0表示不限制个数
        /// </summary>
        public int Backups { get; set; }

        private readonly bool _isFile = false;

        /// <summary>
        /// 是否当前进程的第一次写日志
        /// </summary>
        private bool _isFirst = false;

        internal TextFileLogger(string path, bool isfile, string fileFormat = null)
        {
            LogPath = path;
            _isFile = isfile;

            if (!fileFormat.IsNullOrEmpty())
                FileFormat = fileFormat;

            FileFormat = Config.LoggerOptions.FileLoggerFormat;
            MaxBytes = Config.LoggerOptions.FileLoggerMaxBytes;
            Backups = Config.LoggerOptions.FileLoggerBackups;

            _Timer = new Timer(DoWriteAndClose, null, 0_000, 5_000);
        }


        private static readonly ConcurrentDictionary<string, TextFileLogger> cache = new ConcurrentDictionary<string, TextFileLogger>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 每个目录的日志实例
        /// </summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <param name="fileFormat"></param>
        /// <returns></returns>
        public static TextFileLogger Create(string path = null, string fileFormat = null)
        {
            if (path.IsNullOrEmpty()) path = Config.LoggerOptions.FileLoggerPath;

            var key = (path + fileFormat).ToLower();
            return cache.GetOrAdd(key, k => new TextFileLogger(path, false, fileFormat));
        }

        /// <summary>
        /// 每个目录的日志实例
        /// </summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <returns></returns>
        public static TextFileLogger CreateFile(string path)
        {
            if (path.IsNullOrEmpty()) throw new ArgumentNullException(nameof(path));

            return cache.GetOrAdd(path, k => new TextFileLogger(k, true));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _Timer.Dispose();

            // 销毁前把队列日志输出
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0) WriteAndClose(DateTime.MinValue);
        }

        public override void Release()
        {
            this.Dispose();
        }

        private StreamWriter LogWriter;
        private string CurrentLogFile;

        private StreamWriter InitLog()
        {
            var logfile = GetLogFile();
            logfile.EnsureDirectory(true);

            var stream = new FileStream(logfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var writer = new StreamWriter(stream, Encoding.UTF8);

            // 写日志头
            if (!_isFirst)
            {
                _isFirst = true;

                // 因为指定了编码，比如UTF8，开头就会写入3个字节，所以这里不能拿长度跟0比较
                if (writer.BaseStream.Length > 10) writer.WriteLine();

                writer.Write(GetHead());
            }

            CurrentLogFile = logfile;
            return LogWriter = writer;
        }

        /// <summary>获取日志文件路径</summary>
        /// <returns></returns>
        private string GetLogFile()
        {
            // 单日志文件
            if (_isFile) return LogPath.GetBasePath();

            // 目录多日志文件
            var logfile = LogPath.CombinePath(string.Format(FileFormat, DateTime.Now, Level)).GetBasePath();

            // 是否限制文件大小
            if (MaxBytes == 0) return logfile;

            // 找到今天第一个未达到最大上限的文件
            var max = MaxBytes * 1024L * 1024L;
            var ext = Path.GetExtension(logfile);
            var name = logfile.TrimEnd(ext);
            for (var i = 1; i < 1024; i++)
            {
                if (i > 1) logfile = $"{name}_{i}{ext}";

                var fi = logfile.AsFile();
                if (!fi.Exists || fi.Length < max) return logfile;
            }

            return null;
        }

        private readonly Timer _Timer;
        private readonly ConcurrentQueue<string> _Logs = new ConcurrentQueue<string>();
        private volatile int _logCount;
        private int _writing;
        private DateTime _NextClose;

        /// <summary>写文件</summary>
        protected virtual void WriteFile()
        {
            var writer = LogWriter;

            var now = DateTime.Now;
            if (!_isFile && GetLogFile() != CurrentLogFile)
            {
                writer?.Dispose();
                writer = null;
            }

            // 初始化日志读写器
            if (writer == null) writer = InitLog();

            // 依次把队列日志写入文件
            while (_Logs.TryDequeue(out var str))
            {
                Interlocked.Decrement(ref _logCount);

                // 写日志。TextWriter.WriteLine内需要拷贝，浪费资源
                //writer.WriteLine(str);
                writer.Write(str);
                writer.WriteLine();
            }

            // 写完一批后，刷一次磁盘
            writer?.Flush();

            // 连续5秒没日志，就关闭
            _NextClose = now.AddSeconds(15);
        }

        /// <summary>关闭文件</summary>
        private void DoWriteAndClose(object state)
        {
            // 同步写日志
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0) WriteAndClose(_NextClose);

            // 检查文件是否超过上限
            if (!_isFile && Backups > 0)
            {
                // 判断日志目录是否已存在
                var di = LogPath.GetBasePath().AsDirectory();
                if (di.Exists)
                {
                    // 删除*.del
                    try
                    {
                        var dels = di.GetFiles("*.del");
                        if (dels != null && dels.Length > 0)
                        {
                            foreach (var item in dels)
                            {
                                item.Delete();
                            }
                        }
                    }
                    catch { }

                    var ext = Path.GetExtension(FileFormat);
                    var fis = di.GetFiles("*" + ext);
                    if (fis != null && fis.Length > Backups)
                    {
                        // 删除最旧的文件
                        var retain = fis.Length - Backups;
                        fis = fis.OrderBy(e => e.CreationTime).Take(retain).ToArray();
                        foreach (var item in fis)
                        {
                            WriteLog(LogLevel.Info, string.Format("日志文件达到上限 {0}，删除 {1}，大小 {2:n0}Byte", Backups, item.Name, item.Length), null);
                            try
                            {
                                item.Delete();
                            }
                            catch
                            {
                                item.MoveTo(item.FullName + ".del");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>写入队列日志并关闭文件</summary>
        protected virtual void WriteAndClose(DateTime closeTime)
        {
            try
            {
                // 处理残余
                var writer = LogWriter;
                if (!_Logs.IsEmpty) WriteFile();

                // 连续5秒没日志，就关闭
                if (writer != null && closeTime < DateTime.Now)
                {
                    writer.Dispose();
                    LogWriter = null;
                }
            }
            finally
            {
                _writing = 0;
            }
        }



        protected override void WriteLog(LogLevel level, string message, Exception ex)
        {
            if (_logCount > 100) return;

            var e = WriteLogEventArgs.Current.Set(level).Set(message, ex);

            // 推入队列
            _Logs.Enqueue(e.ToString());
            Interlocked.Increment(ref _logCount);

            // 异步写日志，实时。即使这里错误，定时器那边仍然会补上
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        WriteFile();
                    }
                    finally
                    {
                        _writing = 0;
                    }
                });
            }
        }
    }
}
