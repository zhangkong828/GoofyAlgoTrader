using System.Runtime.InteropServices;

namespace GoofyAlgoTrader
{
    public class Runtime
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
    }
}
