# Broadcast Protocols for Teams (WIP)

TBC

## Exploring the repository

The repository is structured in the following directories:
- **src**: Contains the source code of the application.
    - **BotService**: Contains the core component that connects to the meeting and extracts and injects media feeds from and to the meeting.
    - **ManagementApi**: Contains the main API that is used to interact with the service.
    - **OrchestratorFunction**: Contains the Azure function in charge of managing the status of the VMs.
- **docs**: Contains the documentation on the solution (TBC).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE) license.

## Acknowledgments

This project was implemented using the samples in the [Microsoft Graph Communications API and Samples](https://github.com/microsoftgraph/microsoft-graph-comms-samples) repository as a base. 

The architecture used in the solution was inspired by the samples in [Azure-Samples/PartitionedRepository](https://github.com/Azure-Samples/PartitionedRepository) and [ShawnShiSS/clean-architecture-azure-cosmos-db](https://github.com/ShawnShiSS/clean-architecture-azure-cosmos-db).

---

# Original README - Things to review

> This repo has been populated by an initial template to help get you started. Please
> make sure to update the content to build a great experience for community-building.

As the maintainer of this project, please make a few updates:

- Improving this README.MD file to provide a great experience
- Updating SUPPORT.MD with content about this project's support experience
- Understanding the security reporting process in SECURITY.MD
- Remove this section from the README
