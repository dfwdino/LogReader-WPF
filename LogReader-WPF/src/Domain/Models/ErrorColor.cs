using LogReader_WPF.src.Domain.Interfaces;

namespace LogReader_WPF.src.Domain.Models
{
    public record ErrorColor : IErrorColor
    {
        public ErrorColor(string errorname, string color)
        {
            Name = errorname;
            Color = color;
        }

        public string Name { get; set; }
        public string Color { get; set; }


    }
}
