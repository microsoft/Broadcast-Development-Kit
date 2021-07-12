// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Domain.Enums
{
    /// <summary>
    ///     An enumeration to encapsulate Enums in the solution.
    /// </summary>
    /// <remarks>
    ///     ProvisioningStateType type class should have a protected constructor to encapsulate known enum types
    ///     this is currently not possible as Cosmos DB uses this constructor to map the document into the corresponding type.
    /// </remarks>
    public class ProvisioningStateType : Enumeration
    {
        public static readonly ProvisioningStateType Provisioning = new ProvisioningStateType(0, nameof(Provisioning));
        public static readonly ProvisioningStateType Provisioned = new ProvisioningStateType(1, nameof(Provisioned));
        public static readonly ProvisioningStateType Deprovisioning = new ProvisioningStateType(2, nameof(Deprovisioning));
        public static readonly ProvisioningStateType Deprovisioned = new ProvisioningStateType(3, nameof(Deprovisioned));
        public static readonly ProvisioningStateType Error = new ProvisioningStateType(4, nameof(Error));
        public static readonly ProvisioningStateType Unknown = new ProvisioningStateType(5, nameof(Unknown));

        public ProvisioningStateType()
        {
        }

        public ProvisioningStateType(int id, string name)
            : base(id, name)
        {
        }
    }
}
