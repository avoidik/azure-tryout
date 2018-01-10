using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ConsoleApp2
{
    class Program
    {
        private static void insertCustomer(CloudTable cloudTable, Customer customer)
        {
            TableOperation insert = TableOperation.InsertOrMerge(customer);
            cloudTable.Execute(insert);
        }

        private static Customer insertCustomer(CloudTable cloudTable, string name, string email)
        {
            Customer customer = new Customer(name, email);
            TableOperation insert = TableOperation.InsertOrMerge(customer);
            cloudTable.Execute(insert);
            return customer;
        }

        private static Customer getCustomer(CloudTable cloudTable, Customer customer)
        {
            TableOperation retrieve = TableOperation.Retrieve<Customer>(customer.PartitionKey, customer.RowKey);
            var result = cloudTable.Execute(retrieve);

            Console.WriteLine(((Customer)result.Result).Name);
            return (Customer)result.Result;
        }

        private static Customer getCustomer(CloudTable cloudTable, string partitionKey, string rowKey)
        {
            TableOperation retrieve = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            var result = cloudTable.Execute(retrieve);

            return (Customer)result.Result;
        }

        private static void listCustomers(CloudTable cloudTable)
        {
            TableQuery<Customer> query = new TableQuery<Customer>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "local"));

            Console.WriteLine("List of customers:");
            foreach(Customer c in cloudTable.ExecuteQuery(query))
            {
                Console.WriteLine(c.Name);
            }
            Console.WriteLine();
        }

        private static Customer updateCustomer(CloudTable cloudTable, string name, string email)
        {
            Customer customer = new Customer(name, email);
            TableOperation replace = TableOperation.InsertOrReplace(customer);
            cloudTable.Execute(replace);
            return customer;
        }

        private static void updateCustomer(CloudTable cloudTable, Customer customer)
        {
            TableOperation replace = TableOperation.InsertOrReplace(customer);
            cloudTable.Execute(replace);
        }

        private static void deleteCustomer(CloudTable cloudTable, Customer customer)
        {
            TableOperation delete = TableOperation.Delete(customer);
            cloudTable.Execute(delete);
        }

        private static void singleOperation(CloudTable cloudTable)
        {
            Customer c1 = new Customer("John Doe", "doe@localhost");
            Customer c2 = new Customer("Bob Smith", "bob@localhost");
            Customer c3;

            List<Customer> list = new List<Customer>();
            list.Add(c1);
            list.Add(c2);

            insertCustomer(cloudTable, c1);
            insertCustomer(cloudTable, c2);

            listCustomers(cloudTable);

            c3 = getCustomer(cloudTable, "local", c2.Email);
            c3.Name = "Sally Noel";
            updateCustomer(cloudTable, c3);

            deleteCustomer(cloudTable, c1);
        }

        private static void batchOperationInsert(CloudTable cloudTable)
        {
            TableBatchOperation batch = new TableBatchOperation();

            Customer c1 = new Customer("John Doe", "doe@localhost");
            Customer c2 = new Customer("Bob Smith", "bob@localhost");
            Customer c3 = new Customer("Sally Noel", "sally@localhost");

            batch.InsertOrReplace(c1);
            batch.InsertOrReplace(c2);
            batch.InsertOrReplace(c3);

            cloudTable.ExecuteBatch(batch);
        }

        private static void batchOperationDelete(CloudTable cloudTable)
        {
            TableBatchOperation batch = new TableBatchOperation();

            Customer c1 = new Customer("John Doe", "doe@localhost");
            Customer c2 = new Customer("Bob Smith", "bob@localhost");
            Customer c3 = new Customer("Sally Noel", "sally@localhost");

            batch.Delete(c1);
            batch.Delete(c2);
            batch.Delete(c3);

            cloudTable.ExecuteBatch(batch);
        }

        static void Main(string[] args)
        {
            String connStr = CloudConfigurationManager.GetSetting("StorageConnection");
            CloudStorageAccount cloudStorage = CloudStorageAccount.Parse(connStr);
            CloudTableClient cloudClient = cloudStorage.CreateCloudTableClient();
            CloudTable cloudTable = cloudClient.GetTableReference("customer");
            cloudTable.CreateIfNotExists();

            batchOperationInsert(cloudTable);
            batchOperationDelete(cloudTable);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(false);
        }
    }
}
