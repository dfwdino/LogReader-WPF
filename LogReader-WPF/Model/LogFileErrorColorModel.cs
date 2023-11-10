using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LogReader_WPF.Models
{
    public class LogFileErrorColorModel
    {
        public List<ErrorColer> ErrorColors { get; set; }

        public class ErrorColer
        {
            public string ErrorName { get; set; }
            public string ErrorColor { get; set; }
        }
    }
}
