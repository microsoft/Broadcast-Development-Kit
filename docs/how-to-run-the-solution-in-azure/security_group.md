# Security Group

## Getting Started

The Security Group will be used to grant access and permissions to predefined users to the application. 

To create the **Security Group** and **add members**, review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-groups-create-azure-portal#create-a-basic-group-and-add-members).

### Settings:

- ***GroupType:*** Select the group type **Security**. 
- ***Name:*** A meaningful name.
- ***Membership type:*** Select **Assigned** membership for the group.

> NOTE: This is not an application but an Azure AD security group. Only people added as a **member** of this group will be able to operate the solution through the Management API. 

## Add Member to the Security Group
Once the **Security Group** has been created, it is necessary to add the members of the group that will be able to operate the solution. To add members, it is suggested to carry out the following steps:
1. Enter the **Security Group** created and select the *Members* option on the left panel.
![Add Members](images/security_group_add_members.png)
1. Then click on the **+ Add members** button and start searching for members to add to your group, and click on the **Select** button to add them.
![](images/security_group_select_members.png)

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)