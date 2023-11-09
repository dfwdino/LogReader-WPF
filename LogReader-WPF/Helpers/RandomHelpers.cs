using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogReader_WPF
{
    internal class RandomHelpers
    {
        public static Version GetCurrentVerion() => Assembly.GetEntryAssembly().GetName().Version;
    }
}
