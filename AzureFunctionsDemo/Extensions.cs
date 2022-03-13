using System;
using AzureFunctionsDemo.Entities;
using AzureFunctionsDemo.Models;

namespace AzureFunctionsDemo
{
    public static class Extensions
    {
        public static CustomerTable ToEntity(this Customer customer)
        {
            return new CustomerTable
            {
                PartitionKey = "Customer",
                RowKey = customer.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                Name = customer.Name,
                Address = customer.Address,
                Iban = customer.Iban.ToUpper(),
            };
        }

        public static Customer ToDto(this CustomerTable customer)
        {
            return new Customer
            {
                Id = Guid.Parse(customer.RowKey),
                Name = customer.Name,
                Address = customer.Address,
                Iban = customer.Iban,
            };
        }

        public static TransactionTable ToEntity(this Transaction transaction)
        {
            return new TransactionTable
            {
                PartitionKey = "Transaction",
                RowKey = transaction.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                Amount = transaction.Amount,
                Description = transaction.Description,
                ExecutionDate = transaction.ExecutionDate,
                CreditorIban = transaction.CreditorIban.ToUpper(),
                DebtorIban = transaction.DebtorIban.ToUpper(),
            };
        }

        public static Transaction ToDto(this TransactionTable transaction)
        {
            return new Transaction
            {
                Id = Guid.Parse(transaction.RowKey),
                Amount = transaction.Amount,
                Description = transaction.Description,
                ExecutionDate = transaction.ExecutionDate,
                CreditorIban = transaction.CreditorIban,
                DebtorIban = transaction.DebtorIban,
            };
        }
    }
}
