using LogReader_WPF.src.Domain.Interfaces;

namespace LogReader_WPF.src.Domain.Models
{
    public class LogEntry : ILogEntry
    {
        public required string Content { get; set; }
        public bool IsError { get; set; } = false;
        public bool IsWarning { get; set; } = false;
    }
}
