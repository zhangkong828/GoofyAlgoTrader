using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    class Globals
    {
    }

    public enum Language
    {
        /// <summary>
        /// C# Language Project
        /// </summary>
        [EnumMember(Value = "C#")]
        CSharp,

        /// <summary>
        /// FSharp Project
        /// </summary>
        [EnumMember(Value = "F#")]
        FSharp,

        /// <summary>
        /// Visual Basic Project
        /// </summary>
        [EnumMember(Value = "VB")]
        VisualBasic,

        /// <summary>
        /// Java Language Project
        /// </summary>
        [EnumMember(Value = "Ja")]
        Java,

        /// <summary>
        /// Python Language Project
        /// </summary>
        [EnumMember(Value = "Py")]
        Python
    }

    public enum Resolution
    {
        /// Tick Resolution (1)
        Tick,
        /// Second Resolution (2)
        Second,
        /// Minute Resolution (3)
        Minute,
        /// Hour Resolution (4)
        Hour,
        /// Daily Resolution (5)
        Daily
    }
}
