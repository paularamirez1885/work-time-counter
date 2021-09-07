using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using work_time_counter.Functions.Entities;

namespace work_time_counter.Functions.Functions
{
    public static class ConsolidateTimeFunction
    {
        [FunctionName("ConsolidateTimeFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo consolidateTimer,
            [Table("consolidateTime", Connection = "AzureWebJobsStorage")] CloudTable ConsolidateTimeTable,
            [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
            ILogger log)
        {
            string filterBool = TableQuery.GenerateFilterConditionForBool("consolidate", QueryComparisons.Equal, false);
            string filterInt = TableQuery.GenerateFilterConditionForInt("type", QueryComparisons.Equal, 1);
            string filter = TableQuery.CombineFilters(filterBool, TableOperators.And, filterInt);
            TableQuery<TImeRecordEntity> query = new TableQuery<TImeRecordEntity>().Where(filter);
            TableQuerySegment<TImeRecordEntity> consolidatedTime = await recordTimeTable.ExecuteQuerySegmentedAsync(query, null);

            foreach (TImeRecordEntity consoli in consolidatedTime)
            {
                //update consolidate false to true
                TableOperation findOperation = TableOperation.Retrieve<TImeRecordEntity>("RECORDEDTIME", consoli.RowKey);
                TableResult findResult = await recordTimeTable.ExecuteAsync(findOperation);
                TImeRecordEntity timeRecordEntity = (TImeRecordEntity)findResult.Result;
                timeRecordEntity.consolidate = true;
                TableOperation updateOpetation = TableOperation.Replace(timeRecordEntity);
                await recordTimeTable.ExecuteAsync(updateOpetation);

                //create consolidates on new table
                ConsolidatedTimeEntity consolidateEntity = new ConsolidatedTimeEntity
                {
                    idEmployee = consoli.idEmployee,
                    date = DateTime.UtcNow,
                    workedTime = Convert.ToInt32((consoli.dateOut - consoli.dateIn).TotalMinutes),
                    PartitionKey = "CONSOLIDATE",
                    RowKey = Guid.NewGuid().ToString(),
                };
                TableOperation addConsolidate = TableOperation.Insert(consolidateEntity);
                await ConsolidateTimeTable.ExecuteAsync(addConsolidate);
            }
        }
    }
}
