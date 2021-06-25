using System;
using Domain.Enums;

namespace Application.Common.Models
{
    public class ServiceModel
    {
        public string Id { get; set; }

        public string CallId { get; set; }

        public string Name { get; set; }

        public ServiceState State { get; set; }

        public DateTime CreatedAt { get; set; }

        public InfrastructureModel Infrastructure { get; set; }
    }
}
