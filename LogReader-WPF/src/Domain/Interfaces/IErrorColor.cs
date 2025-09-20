using LogReader_WPF.src.Domain.Models;

namespace LogReader_WPF.src.Domain.Interfaces
{
    public interface IErrorColor
    {
        string Color { get; set; }
        string Name { get; set; }

        bool Equals(ErrorColor? other);
        bool Equals(object? obj);
        int GetHashCode();
        string ToString();
    }
}