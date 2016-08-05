using System.Collections.Generic;
using SqlComparer.Web.Models;

namespace SqlComparer.Web.ViewModels
{
    public class LogHistory
    {
        public IEnumerable<LogMessage> LogMessages { get; set; } = new List<LogMessage>();
    }
}
