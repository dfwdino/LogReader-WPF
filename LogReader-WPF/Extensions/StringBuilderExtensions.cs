using System;
using System.Text;

namespace LogReader_WPF.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder RemoveLastNewLine(this StringBuilder sb)
        {
            if (sb.Length >= Environment.NewLine.Length)
            {
                sb.Length -= Environment.NewLine.Length;
            }
            return sb;
        }
    }
}
