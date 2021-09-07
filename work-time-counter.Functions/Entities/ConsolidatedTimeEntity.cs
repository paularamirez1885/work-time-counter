using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace work_time_counter.Functions.Entities
{
    public class ConsolidatedTimeEntity : TableEntity
    {
        public int idEmployee { get; set; }
        public DateTime date { get; set; }
        public int workedTime { get; set; }
    }
}
