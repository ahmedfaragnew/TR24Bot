using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;

namespace ContosoElectronics
{
    public class Storage
    {
        CloudTableClient tableClient;
        CloudTable table;
        public Storage()
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = 
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            table = tableClient.GetTableReference("Users");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
        }

        public void AddUser(string ID, string FirstName, string LastName, string Email)
        {
            // Create a new user entity.
            Users userRecord = new Users(LastName, FirstName, ID, Email,"");

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(userRecord);

            // Execute the insert operation.
            table.Execute(insertOperation);

        }

        public void AddUser(string ID, string name, string Email)
        {
            // Create a new user entity.
            Users userRecord = new Users(name, ID, Email, "");

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(userRecord);

            // Execute the insert operation.
            table.Execute(insertOperation);

        }

        public Users GetUser(string ID, string name)
        {
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<Users>(name, ID);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
                return ((Users)retrievedResult.Result);
            else
                return null;

        }

        public List<string> GetUsers()
        {
            List<string> Users = new List<string>();
            // Create the CloudTable object that represents the "people" table.

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<Users> query = new TableQuery<Users>();

            // Print the fields for each customer.
            foreach (Users user in table.ExecuteQuery(query))
            {
                Users.Add(user.Name);
            }
            return Users;
        }
    }

    public class Users : TableEntity
    {
        public Users(string lastName, string firstName, string id, string email, string phone)
        {
            this.PartitionKey = lastName;
            this.RowKey = id;
            Name = firstName + " " + lastName;
            ID = id;
            Email = email;
            PhoneNumber = phone;
        }

        public Users(string name, string id, string email, string phone)
        {
            this.PartitionKey = name;
            this.RowKey = id;
            Name = name;
            ID = id;
            Email = email;
            PhoneNumber = phone;
        }

        public Users() { }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}