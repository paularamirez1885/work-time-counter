using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace work_time_counter.Functions.Entities
{
    public class TImeRecordEntity: TableEntity
    {
        public int idEmployee { get; set; }
        public DateTime dateIn { get; set; }
        public DateTime dateOut { get; set; }
        public int type { get; set; }
        public bool consolidate { get; set; }
    }
}
