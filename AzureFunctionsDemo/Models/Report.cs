using System.Collections.Generic;

namespace AzureFunctionsDemo.Models
{
    public class Report
    {
        public Customer Customer { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
