namespace LogReader_WPF.src.Domain.Interfaces
{
    public interface ILogEntry
    {
        string Content { get; set; }
        bool IsError { get; set; }
        bool IsWarning { get; set; }
    }
}