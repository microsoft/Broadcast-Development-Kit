using System;
using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities
{
    public class Service : BaseEntity
    {
        public string CallId { get; set; }

        public string Name { get; set; }

        public ServiceState State { get; set; }

        public DateTime CreatedAt { get; set; }

        public Infrastructure Infrastructure { get; set; }
    }
}