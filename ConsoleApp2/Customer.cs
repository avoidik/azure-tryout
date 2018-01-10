using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Customer : TableEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public Customer() { }

        public Customer(string name, string email)
        {
            this.PartitionKey = "local";
            this.RowKey = email;
            this.Name = name;
            this.Email = email;
        }
    }
}
