using Microsoft.Azure.Cosmos.Table;

namespace AzureFunctionsDemo.Entities 
{
    public class CustomerTable : TableEntity
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Iban { get; set; }
    }
}
