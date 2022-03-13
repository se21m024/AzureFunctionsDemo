using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureFunctionsDemo.Entities
{
    public class TransactionTable : TableEntity
    {
        public double Amount { get; set; }

        public DateTimeOffset ExecutionDate { get; set; }

        public string Description { get; set; }

        public string CreditorIban { get; set; }

        public string DebtorIban { get; set; }
    }
}
