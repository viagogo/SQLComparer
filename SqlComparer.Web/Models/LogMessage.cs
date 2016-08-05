using System;
using System.Globalization;

namespace SqlComparer.Web.Models
{
    public class LogMessage
    {
        public DateTime EventTime { get; set; }
        public string Source { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }

        public static LogMessage FromString(string input)
        {
            var elements = input.Split('|');
            return new LogMessage
            {
                EventTime = DateTime.ParseExact(elements[0], "[yyyy/MM/dd HH:mm:ss.fff]", CultureInfo.InvariantCulture),
                Source = elements[1],
                Severity = elements[2],
                Message = elements[3]
            };
        }
    }
}
