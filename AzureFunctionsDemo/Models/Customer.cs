using System;

namespace AzureFunctionsDemo.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Address { get; set; }

        public string  Iban { get; set; }
    }
}
