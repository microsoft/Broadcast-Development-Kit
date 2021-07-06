# Cosmos DB Database

## Getting Started

The **Azure Cosmos DB** database will be used to store the application state when the solution is used in a call, as well as to store Service's state and Stream Injections data.


To create the Azure Cosmos DB, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/create-cosmosdb-resources-portal#create-an-azure-cosmos-db-account).

### Settings:

- ***Select API option:*** choose Core (SQL).
- ***Resource Group:*** Select the [resource group](resource_group.md) created for the solution architecture.
- ***Account Name***: a meaningful name. 
- ***Location***: same region as the rest of the resources. 
- ***Capacity mode***: Provisioned throughput. 
- ***Apply Free Tier Discount***: Apply only if there is no other Cosmos DB using it in the subscription.

Leave the rest of the settings as-is.

> **NOTE:**  The database and the container will be created when running the solution. However, the container has created without shared between them. To save cost, you can create this database (delete if was created) create manually, by following the steps below.

Once the database is created, browse to the **Data Explorer** in the left panel of the account configuration and create a new database with the following settings: 

- Database Id: A meaningful name. 
- Throughput: Manual – 400 RU/s. 
    > Note: To keep the costs down, you are setting the RU to the lowest amount possible.

Click on **OK** button to create the database.

After the database is created, it is necessary to add all the containers needed by the solution. The following list shows the name of the containers to be created and the corresponding partition key name:

- ***Call***
    - Partition Key: /id
- ***ParticipantStream***
    - Partition Key: /id
- ***Service***
    - Partition Key: /id
- ***Stream***
    - Partition Key: /id

To create those containers, please follow the next steps:

1. In the `Data explorer` view for Cosmos DB in the Azure Portal, select `New Container`.
1. Fill the `New Container` blade displayed with the following values:
    - ***Database id***: Check use existing and select the one created in the previous step.
    - ***Container id***: Write the name of the container (e.g. *Call*).
    - ***Partition key***: Write the name of the partition key (e.g. for Call container the partition key name is /id).
    - ***Provision dedicated throughput for this container***: Keep unchecked to share the database throughput between all the containers created.
1. Click **OK** button to create the container.

The images below show the steps from the Azure Portal.

#### Create `New Container` button.

![Add new container](./images/cosmos_db_create_new_container.png)

#### Create `New Container` blade displayed.

![Fill the data to create the container](./images/cosmos_db_create_new_container_blade.png)

>NOTE: The steps described above can be done through the Microsoft Azure Storage explorer. 


Finally, go to the resource blade on the left, go to the setting section click on Keys. Copy and save the `URI` and `PRIMARY KEY` from the values displayed. Those values are required along with the database name to complete the configuration of the Bot Service, Bot Orchestrator and Management API.

![Uri and primary key values to copy](./images/cosmos_db_key_and_connection_string.png)

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)
