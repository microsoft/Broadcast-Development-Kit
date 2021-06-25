using Domain.Enums;

namespace Domain.Entities
{
    public class ProvisioningDetails
    {
        public string Message { get; set; }

        public ProvisioningStateType State { get; set; }
    }
}
