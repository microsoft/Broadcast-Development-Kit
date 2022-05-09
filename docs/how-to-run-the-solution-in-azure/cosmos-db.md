# Cosmos DB

The **Azure Cosmos DB** database will be used to store the `Broadcast Development Kit` data. To create the Azure Cosmos DB, please review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/create-cosmosdb-resources-portal#create-an-azure-cosmos-db-account), and use the following settings:

- ***Select API option:*** choose Core (SQL).
- ***Resource Group:*** Select the resource group created for the main resources.
- ***Account Name***: a meaningful name.
- ***Location***: same region as the rest of the resources.
- ***Capacity mode***: Serverless.
- ***Apply Free Tier Discount***: Apply only if there is no other Cosmos DB using it in the subscription.

Leave the rest of the settings as-is.

After creating the Cosmos DB account, navigate to the **Data Explorer** option on the resource blade you have on the left and create new database and the following containers ([How to add a database and a container](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/create-cosmosdb-resources-portal#add-a-database-and-a-container)):

- ***Call***
  - Partition Key: /id
- ***ParticipantStream***
  - Partition Key: /id
- ***Service***
  - Partition Key: /id
- ***Stream***
  - Partition Key: /id

Finally, navigate to the **Keys** option you have under the **Settings** section on the resource blade, take note of the `PRIMARY KEY` and add it to the key vault as a secret with the following name:
`Settings--CosmosDbConfiguration--PrimaryKey`.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Storage Account →](storage-account.md)
