using Domain.Enums;

namespace Application.Common.Models
{
    public class ProvisioningDetailsModel
    {
        public ProvisioningStateType State { get; set; }
        public string Message { get; set; }
    }
}
