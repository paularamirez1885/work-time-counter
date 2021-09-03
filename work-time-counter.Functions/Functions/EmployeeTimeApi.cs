using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using work_time_counter.Common.Models;
using work_time_counter.Common.Responses;
using work_time_counter.Functions.Entities;

namespace work_time_counter.Functions.Functions
{
    public static class EmployeeTimeApi
    {
        [FunctionName(nameof(CreateRecordTime))]
        public static async Task<IActionResult> CreateRecordTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recordTime")] HttpRequest req,
            [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
            ILogger log)
        {
            log.LogInformation("create new time record");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TimeRecord record = JsonConvert.DeserializeObject<TimeRecord>(requestBody);

            if (record == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "To record an event, fields can't be empty"
                });
            }

            if (record != null && record.idEmployee == 0)
            {
                log.LogInformation($"id employeee {record.idEmployee}");
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "To record an event, have to send id employee"
                });
            }

            TImeRecordEntity timeRecordEntity = new TImeRecordEntity
            {
                idEmployee = record.idEmployee,
                dateIn = DateTime.UtcNow,
                dateOut = DateTime.UtcNow,
                type = 0,
                consolidate = false,
                ETag = "*",
                PartitionKey = "RECORDEDTIME",
                RowKey = Guid.NewGuid().ToString(),
            };

            TableOperation addOperation = TableOperation.Insert(timeRecordEntity);
            await recordTimeTable.ExecuteAsync(addOperation);
            string message = "New time saved succesfull";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                Result = timeRecordEntity
            });
        }


        [FunctionName(nameof(UpdateRecordTime))]
        public static async Task<IActionResult> UpdateRecordTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "recordTime/{id}")] HttpRequest req,
            [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update record with id: {id}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TimeRecord record = JsonConvert.DeserializeObject<TimeRecord>(requestBody);

            TableOperation findOperation = TableOperation.Retrieve<TImeRecordEntity>("RECORDEDTIME", id);
            TableResult findResult = await recordTimeTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Register not found."
                });
            }

            if (record.type == 0)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Is not posible save a get in register whitout get out"
                });
            }

            TImeRecordEntity timeRecordEntity = (TImeRecordEntity)findResult.Result;
            timeRecordEntity.consolidate = record.consolidate;
            if (record.type != 0)
            {
                timeRecordEntity.type = record.type;
                timeRecordEntity.dateOut = DateTime.UtcNow;
            }

            TableOperation addOperation = TableOperation.Replace(timeRecordEntity);
            await recordTimeTable.ExecuteAsync(addOperation);
            string message = "New time saved succesfull";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                Result = timeRecordEntity
            });
        }

        [FunctionName(nameof(GetAllRegister))]
        public static async Task<IActionResult> GetAllRegister(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "listRegister")] HttpRequest req,
           [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
           ILogger log)
        {
            log.LogInformation("Get all registers.");

            TableQuery<TImeRecordEntity> query = new TableQuery<TImeRecordEntity>();
            TableQuerySegment<TImeRecordEntity> list = await recordTimeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "list of registers.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                Result = list
            });
        }

        [FunctionName(nameof(GetRegisterById))]
        public static IActionResult GetRegisterById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timeRegister/{id}")] HttpRequest req,
            [Table("recordTime", "RECORDEDTIME", "{id}", Connection = "AzureWebJobsStorage")] TImeRecordEntity timeRecordEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get time register by id: {id}");

            if (timeRecordEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Todo not found."
                });
            }

            string message = $"Register: {timeRecordEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                Result = timeRecordEntity
            });
        }

        [FunctionName(nameof(DeleteRegister))]
        public static async Task<IActionResult> DeleteRegister(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "register/{id}")] HttpRequest req,
            [Table("recordTime", "RECORDEDTIME", "{id}", Connection = "AzureWebJobsStorage")] TImeRecordEntity timeRecordEntity,
            [Table("recordTime", Connection = "AzureWebJobsStorage")] CloudTable recordTimeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete register: {id}, received.");

            if (timeRecordEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "register not found."
                });
            }

            await recordTimeTable.ExecuteAsync(TableOperation.Delete(timeRecordEntity));
            string message = $"register: {timeRecordEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                Result = timeRecordEntity
            });
        }
    }
}
