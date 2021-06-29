# Cosmos DB Database

## Getting Started
The **Azure Cosmos DB** database will be used to save the application status when the solution is being used in a call.

To create the Azure Cosmos DB, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/create-cosmosdb-resources-portal#create-an-azure-cosmos-db-account).

### Seetings:

- For the API selection, choose Core (SQL). 
- Name: a meaningfull name. 
- Region: same region as the rest of the resources. 
- Capacity mode: Provisioned throughput. 
- Apply Free Tier Discount: Apply only if there is no other Cosmos DB using it in the subscription.

Leave the rest of the settings as-is. Once the account is created, browse to the **Data Explorer** menu in the account configuration and create a new Database with the following settings: 

- Database Id: dronetx-database (or any other meaningful name). 
- Throughput: Manual – 400 RU/s. 
    > Note: To keep the costs down for this PoC we are setting the RU to the lowest amount possible. 

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)