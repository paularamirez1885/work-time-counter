using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using work_time_counter.Functions.Entities;

namespace work_time_counter.Functions.Functions
{
    public static class ConsolidateTimeFunction
    {
        [FunctionName("ConsolidateTimeFunction")]
        public static async Task  Run([TimerTrigger("0/5 0/5 0 ? * * *")]TimerInfo myTimer,
            [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
            ILogger log)
        {
            log.LogInformation($"Consolidate workin time fuction excecuted at: {DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("consolidate", QueryComparisons.Equal, true);
            TableQuery<TImeRecordEntity> query = new TableQuery<TImeRecordEntity>().Where(filter);
            TableQuerySegment<TImeRecordEntity> consolidatedTime = await recordTimeTable.ExecuteQuerySegmentedAsync(query, null);
            int consolidated = 0;
            foreach (TImeRecordEntity consoli in consolidatedTime)
            {
                await recordTimeTable.ExecuteAsync(TableOperation.Retrieve("consolidate", "true"));
            }
            log.LogInformation($"Consolidate workin paula ramirez la mejor: {DateTime.Now}");


        }
    }
}
