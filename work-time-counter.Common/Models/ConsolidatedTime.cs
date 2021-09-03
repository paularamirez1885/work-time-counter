using System;

namespace work_time_counter.Common.Models
{
    public class ConsolidatedTime
    {
        public int idEmployee { get; set; }
        public DateTime date { get; set; }
        public TimeSpan workedTime { get; set; }
    }
}
