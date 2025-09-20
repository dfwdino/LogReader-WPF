using LogReader_WPF.src.Domain.Models;
using System.Collections.Generic;

namespace LogReader_WPF.src.Application.Interfaces
{
    public interface ILogFileErrorColorModel
    {
        List<ErrorColor> ErrorColors { get; set; }
    }
}