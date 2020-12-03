using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public static class PathExtension
    {
        public static string BasePath { get; set; }

        static PathExtension()
        {
            var dir = "";
            // 命令参数
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].EqualIgnoreCase("-BasePath", "--BasePath") && i + 1 < args.Length)
                {
                    dir = args[i + 1];
                    break;
                }
            }

            // 环境变量
            if (dir.IsNullOrEmpty()) dir = Environment.GetEnvironmentVariable("BasePath");

            // 最终取应用程序域。Linux下编译为单文件时，应用程序释放到临时目录，应用程序域基路径不对，当前目录也不一定正确，唯有进程路径正确
            if (dir.IsNullOrEmpty()) dir = AppDomain.CurrentDomain.BaseDirectory;

            BasePath = GetPath(dir, 1);
        }

        private static string GetPath(string path, int mode)
        {
            // 处理路径分隔符，兼容Windows和Linux
            var sep = Path.DirectorySeparatorChar;
            var sep2 = sep == '/' ? '\\' : '/';
            path = path.Replace(sep2, sep);

            var dir = "";
            switch (mode)
            {
                case 1:
                    dir = AppDomain.CurrentDomain.BaseDirectory;
                    break;
                case 2:
                    dir = BasePath;
                    break;
                case 3:
                    dir = Environment.CurrentDirectory;
                    break;
                default:
                    break;
            }
            if (dir.IsNullOrEmpty()) return Path.GetFullPath(path);

            // 处理网络路径
            if (path.StartsWith(@"\\", StringComparison.Ordinal)) return Path.GetFullPath(path);

            //if (!Path.IsPathRooted(path))
            //!!! 注意：不能直接依赖于Path.IsPathRooted判断，/和\开头的路径虽然是绝对路径，但是它们不是驱动器级别的绝对路径
            if (/*path[0] == sep ||*/ path[0] == sep2 || !Path.IsPathRooted(path))
            {
                path = path.TrimStart('~');

                path = path.TrimStart(sep);
                path = Path.Combine(dir, path);
            }

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// 获取文件或目录基于应用程序域基目录的全路径，过滤相对目录
        /// </summary>
        /// <remarks>不确保目录后面一定有分隔符，是否有分隔符由原始路径末尾决定</remarks>
        /// <param name="path">文件或目录</param>
        /// <returns></returns>
        public static string GetFullPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            return GetPath(path, 1);
        }

        /// <summary>
        /// 获取文件或目录的全路径，过滤相对目录。用于X组件内部各目录，专门为函数计算而定制
        /// </summary>
        /// <remarks>不确保目录后面一定有分隔符，是否有分隔符由原始路径末尾决定</remarks>
        /// <param name="path">文件或目录</param>
        /// <returns></returns>
        public static string GetBasePath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            return GetPath(path, 2);
        }

        /// <summary>
        /// 确保目录存在，若不存在则创建
        /// </summary>
        /// <remarks>
        /// 斜杠结尾的路径一定是目录，无视第二参数；
        /// 默认是文件，这样子只需要确保上一层目录存在即可，否则如果把文件当成了目录，目录的创建会导致文件无法创建。
        /// </remarks>
        /// <param name="path">文件路径或目录路径，斜杠结尾的路径一定是目录，无视第二参数</param>
        /// <param name="isfile">该路径是否是否文件路径。文件路径需要取目录部分</param>
        /// <returns></returns>
        public static string EnsureDirectory(this string path, bool isfile = true)
        {
            if (string.IsNullOrEmpty(path)) return path;

            path = path.GetFullPath();
            if (File.Exists(path) || Directory.Exists(path)) return path;

            var dir = path;
            // 斜杠结尾的路径一定是目录，无视第二参数
            if (dir[dir.Length - 1] == Path.DirectorySeparatorChar)
                dir = Path.GetDirectoryName(path);
            else if (isfile)
                dir = Path.GetDirectoryName(path);

            /*!!! 基础类库的用法应该有明确的用途，而不是通过某些小伎俩去让人猜测 !!!*/

            //// 如果有圆点说明可能是文件
            //var p1 = dir.LastIndexOf('.');
            //if (p1 >= 0)
            //{
            //    // 要么没有斜杠，要么圆点必须在最后一个斜杠后面
            //    var p2 = dir.LastIndexOf('\\');
            //    if (p2 < 0 || p2 < p1) dir = Path.GetDirectoryName(path);
            //}

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            return path;
        }

        /// <summary>合并多段路径</summary>
        /// <param name="path"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string CombinePath(this string path, params string[] ps)
        {
            if (ps == null || ps.Length < 1) return path;
            if (path == null) path = string.Empty;

            //return Path.Combine(path, path2);
            foreach (var item in ps)
            {
                if (!item.IsNullOrEmpty()) path = Path.Combine(path, item);
            }
            return path;
        }

        /// <summary>路径作为目录信息</summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static DirectoryInfo AsDirectory(this string dir) => new DirectoryInfo(dir.GetFullPath());

        /// <summary>文件路径作为文件信息</summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileInfo AsFile(this string file) => new FileInfo(file.GetFullPath());
    }
}
