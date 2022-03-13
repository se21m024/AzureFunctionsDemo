using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AzureFunctionsDemo.Entities;
using AzureFunctionsDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsDemo
{
    public static class Banking
    {
        [FunctionName("Info")]
        public static async Task<IActionResult> Info(
            [HttpTrigger(AuthorizationLevel.Anonymous, Const.MethodTypes.Get)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Info function called.");
            return new OkObjectResult(
                "This is a .NET 6 Azure Function Demo project from the student se21m024 at the FH Technikum Wien.");
        }

        [FunctionName("Customer")]
        public static async Task<IActionResult> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, Const.MethodTypes.Post)]
            HttpRequest req,
            [Table(Const.Tables.Customers, Connection = Const.StorageAccount)]
            IAsyncCollector<CustomerTable> customerTableCollector,
            [Table(Const.Tables.Customers, Connection = Const.StorageAccount)]
            CloudTable customersTable,
            ILogger log)
        {
            try
            {
                log.LogInformation("Create Customer function called.");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestResult();
                }

                var customer = JsonConvert.DeserializeObject<Customer>(requestBody).ToEntity();

                var filterA = TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, customer.Name);
                var filterB = TableQuery.GenerateFilterCondition("Iban", QueryComparisons.Equal, customer.Iban);

                var query = new TableQuery<CustomerTable>()
                    .Where(TableQuery.CombineFilters(filterA, TableOperators.Or, filterB));

                var existingCustomer = customersTable.ExecuteQuery(query).FirstOrDefault();

                if (existingCustomer is not null)
                {
                    return new ConflictObjectResult($"A customer with the provided name of IBAN already exists.");
                }

                await customerTableCollector.AddAsync(customer);
                return new OkObjectResult(customer);
            }
            catch (Exception e)
            {
                log.LogError($"Failed to create customer: {e}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("Customers")]
        public static async Task<IActionResult> GetCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, Const.MethodTypes.Get)]
            HttpRequest req,
            [Table(Const.Tables.Customers, Connection = Const.StorageAccount)]
            CloudTable customersTable,
            ILogger log)
        {
            try
            {
                log.LogInformation("Get Customers function called.");
                var query = new TableQuery<CustomerTable>();
                var customers = customersTable.ExecuteQuery(query).Select(Extensions.ToDto);
                return new OkObjectResult(customers);
            }
            catch (Exception e)
            {
                log.LogError($"Failed to get customers: {e}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("Transaction")]
        public static async Task<IActionResult> CreateTransaction(
            [HttpTrigger(AuthorizationLevel.Anonymous, Const.MethodTypes.Post)]
            HttpRequest req,
            [Table(Const.Tables.Transactions, Connection = Const.StorageAccount)]
            IAsyncCollector<TransactionTable> transactionTableCollector,
            ILogger log)
        {
            try
            {
                log.LogInformation("Create Transaction called.");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestResult();
                }

                var transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);
                await transactionTableCollector.AddAsync(transaction.ToEntity());
                return new OkObjectResult(transaction);
            }
            catch (Exception e)
            {
                log.LogError($"Failed to create transaction: {e}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("Report")]
        public static async Task<IActionResult> GetReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, Const.MethodTypes.Get)]
            HttpRequest req,
            [Table(Const.Tables.Customers, Connection = Const.StorageAccount)]
            CloudTable customersTable,
            [Table(Const.Tables.Transactions, Connection = Const.StorageAccount)]
            CloudTable transactionsTable,
            ILogger log)
        {
            try
            {
                log.LogInformation("Get Report function called.");
                var iban = req.Query["iban"].ToString()?.Trim().ToUpper();
                var validYear = int.TryParse(req.Query["year"], out var year);
                var validMonth = int.TryParse(req.Query["month"], out var month);

                if (string.IsNullOrWhiteSpace(iban) || !validMonth || !validYear)
                {
                    return new BadRequestResult();
                }

                var customerQuery = new TableQuery<CustomerTable>()
                    .Where(TableQuery.GenerateFilterCondition("Iban", QueryComparisons.Equal, iban));

                customerQuery.TakeCount = 1;
                var customer = customersTable.ExecuteQuery(customerQuery).FirstOrDefault();

                if (customer is null)
                {
                    return new NotFoundObjectResult($"No customer with IBAN <{iban}> could be found.");
                }

                var filterA = TableQuery.GenerateFilterCondition("CreditorIban", QueryComparisons.Equal, iban);
                var filterB = TableQuery.GenerateFilterCondition("DebtorIban", QueryComparisons.Equal, iban);

                var transactionsQuery = new TableQuery<TransactionTable>()
                    .Where(TableQuery.CombineFilters(filterA, TableOperators.Or, filterB));

                var allTransactions = transactionsTable.ExecuteQuery(transactionsQuery);

                var transactions = allTransactions
                    .Where(x => x.ExecutionDate.Year == year && x.ExecutionDate.Month == month);

                var report = new Report
                {
                    Customer = customer.ToDto(),
                    Year = year,
                    Month = month,
                    Transactions = transactions.Select(x => x.ToDto()).ToList(),
                };

                return new OkObjectResult(report);
            }
            catch (Exception e)
            {
                log.LogError($"Failed to get report: {e}");
                return new InternalServerErrorResult();
            }
        }
    }
}
