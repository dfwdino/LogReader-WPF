namespace LogReader_WPF.Model
{
    public class LogEntry
    {
        public required string Content { get; set; }
        public bool IsError { get; set; } = false;
        public bool IsWarning { get; set; } = false;
    }
}
