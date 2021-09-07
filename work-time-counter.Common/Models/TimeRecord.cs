using System;

namespace work_time_counter.Common.Models
{
    public class TimeRecord
    {
        public int idEmployee { get; set; }
        public DateTime dateIn { get; set; }
        public DateTime dateOut { get; set; }
        public int type { get; set; }
        public bool consolidate { get; set; }

    }

}
