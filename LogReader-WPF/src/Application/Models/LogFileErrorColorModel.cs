using LogReader_WPF.src.Application.Interfaces;
using LogReader_WPF.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogReader_WPF.src.Application.Models
{
    public class LogFileErrorColorModel : ILogFileErrorColorModel
    {
        public List<ErrorColor> ErrorColors { get; set; } = new();

        public void AddErrorColor(ErrorColor errorColor)
        {
            ErrorColors.Add(errorColor);
        }

        public ErrorColor? GetColorForError(string errorName)
        {
            return ErrorColors.FirstOrDefault(ec => ec.Name.Equals(errorName, StringComparison.OrdinalIgnoreCase));
        }

    }
}
