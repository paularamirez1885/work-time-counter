using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace work_time_counter.Functions.Entities
{
    public class ConsolidatedTimeEntity: TableEntity
    {
        public int idEmployee { get; set; }
        public DateTime date { get; set; }
        public TimeSpan workedTime { get; set; }
    }
}
