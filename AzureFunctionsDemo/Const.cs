namespace AzureFunctionsDemo
{
    internal static class Const
    {
        public const string StorageAccount = "AzureWebJobsStorage";

        public static class Tables
        {
            public const string Customers = "customers";

            public const string Transactions = "transactions";
        }

        public static class MethodTypes
        {
            public const string Get = "get";

            public const string Post = "post";
        }
    }
}
