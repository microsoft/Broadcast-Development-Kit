"use strict";

import { CosmosClient, Database } from "@azure/cosmos";
import { ArgumentParser } from "argparse";

interface CosmosDbDocument {
    CallId: null;
    Name: string;
    State: number;
    CreatedAt: string;
    Infrastructure: {
        VirtualMachineName: string;
        ResourceGroup: string;
        SubscriptionId: string;
        Id: string;
        PowerState: string;
        IpAddress: null;
        Dns: string;
        ProvisioningDetails: {
          Message: string;
          State: {
              Id: number;
              Name: string;
          }
        };
    };
    id: string;
}

const cosmosDbContainer = "Service";

async function main() {
  const parser = new ArgumentParser({
    add_help: true,
    description:
      "Script to sync directories of static items into CosmosDB container",
  });
  parser.add_argument("--connectionString", {
    dest: "connectionString",
    help: "The endpoint URI of the CosmosDB account",
    metavar: "HOST",
    required: true,
  });
  parser.add_argument("--db", {
    default: "bdk",
    dest: "cosmosDatabase",
    help: "The ID of the CosmosDB database",
    metavar: "ID",
    required: true,
  });
  parser.add_argument("--dns", {
    default: "dns",
    dest: "dns",
    help: "The DNS record name",
    metavar: "DNS",
    required: true,
  });
  parser.add_argument("--vmName", {
    default: "vmName",
    dest: "vmName",
    help: "The VM name",
    metavar: "vmName",
    required: true,
  });
  parser.add_argument("--subscriptionId", {
    default: "subscriptionId",
    dest: "subscriptionId",
    help: "The subscription ID",
    metavar: "SubID",
    required: true,
  });
  parser.add_argument("--vmResourceGroup", {
    default: "vmResourceGroup",
    dest: "vmResourceGroup",
    help: "The VM Resource Group",
    metavar: "vmResourceGroup",
    required: true,
  });

  const args = parser.parse_args();

  const client = new CosmosClient(args.connectionString);

  console.log(
    "Connecting to CosmosDB database",
    args.cosmosDatabase
  );

  const { database } = await client.databases.createIfNotExists({
    id: args.cosmosDatabase,
  });

  const document = {
    CallId: null,
    Name: args.vmName,
    State: 1,
    CreatedAt: "2021-08-01T00:00:00+00:00",
    Infrastructure: {
        VirtualMachineName: args.vmName,
        ResourceGroup: args.vmResourceGroup.toLowerCase(),
        SubscriptionId: args.subscriptionId,
        Id: `/subscriptions/${args.subscriptionId}/resourcegroups/${args.vmResourceGroup.toLowerCase()}/providers/microsoft.compute/virtualmachines/${args.vmName}`,
        PowerState: "PowerState/running",
        IpAddress: null,
        Dns: args.dns,
        ProvisioningDetails: {
            Message: "Service provisioned.",
            State: {
                Id: 1,
                Name: "Provisioned"
            }
        }
    },
    id: "00000000-0000-0000-0000-000000000000"
  }

  await addDocument(database, document);
}


async function addDocument(database: Database, document: CosmosDbDocument) {
  const container = database.container(cosmosDbContainer);

  const item = container.item(document.id, document.id);
  const itemReadResponse = await item.read();
 
  if (itemReadResponse.resource) {
    console.log(
      "Item",
      "does exist in container",
      cosmosDbContainer,
      "-- updating"
    );
    await container.items.upsert(document)
  } else {
    console.log(
      "Item",
      "does not exist in container",
      cosmosDbContainer,
      "-- creating it"
    );
    await container.items.create(document);
  }

}

main().catch((err) => {
  console.error("Unhandled exception");
  console.error(err.stack);
  process.exit(1);
});
