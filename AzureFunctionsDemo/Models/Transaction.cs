using System;

namespace AzureFunctionsDemo.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public double Amount { get; set; }

        public DateTimeOffset ExecutionDate { get; set; }

        public string Description { get; set; }

        public string CreditorIban { get; set; }

        public string DebtorIban { get; set; }
    }
}
