using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GoofyAlgoTrader
{
    public static class Runtime
    {
        /// <summary>
        /// 是否Windows环境
        /// </summary>
        public static bool Windows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// 是否Linux环境
        /// </summary>
        public static bool Linux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// 是否OSX环境
        /// </summary>
        public static bool OSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Get the drive space remaining on windows and linux in MB
        /// </summary>
        public static long DriveSpaceRemaining
        {
            get
            {
                var d = GetDrive();
                return d.AvailableFreeSpace / (1024 * 1024);
            }
        }

        /// <summary>
        /// Get the drive space remaining on windows and linux in MB
        /// </summary>
        public static long DriveSpaceUsed
        {
            get
            {
                var d = GetDrive();
                return (d.TotalSize - d.AvailableFreeSpace) / (1024 * 1024);
            }
        }

        /// <summary>
        /// Total space on the drive
        /// </summary>
        public static long DriveTotalSpace
        {
            get
            {
                var d = GetDrive();
                return d.TotalSize / (1024 * 1024);
            }
        }

        /// <summary>
        /// Get the drive.
        /// </summary>
        /// <returns></returns>
        private static DriveInfo GetDrive()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var drive = Path.GetPathRoot(assembly.Location);
            return new DriveInfo(drive);
        }

        /// <summary>
        /// Gets the amount of private memory allocated for the current process (includes both managed and unmanaged memory).
        /// </summary>
        public static long ApplicationMemoryUsed
        {
            get
            {
                var proc = Process.GetCurrentProcess();
                return proc.PrivateMemorySize64 / (1024 * 1024);
            }
        }

        /// <summary>
        /// Get the RAM used on the machine:
        /// </summary>
        public static long TotalPhysicalMemoryUsed => GC.GetTotalMemory(false) / (1024 * 1024);

        /// <summary>
        /// Total CPU usage as a percentage
        /// </summary>
        public static decimal CpuUsage
        {
            get
            {
                //todo
                return 0;
            }
        }

        /// <summary>
        /// Gets the statistics of the machine, including CPU% and RAM
        /// </summary>
        public static Dictionary<string, string> GetServerStatistics()
        {
            return new Dictionary<string, string>
            {
                { "CPU Usage",CpuUsage.ToString().ToStringInvariant()},
                { "Used RAM (MB)", TotalPhysicalMemoryUsed.ToStringInvariant() },
                { "Total RAM (MB)", "" },
                { "Hostname", Environment.MachineName }
            };
        }
    }
}
