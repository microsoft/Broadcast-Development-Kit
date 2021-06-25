using System;
using Domain.Entities.Base;

namespace Domain.Entities.Audit
{
    public class Audit : BaseEntity
    {
        public Audit(
            string entityType,
            string entityId,
            string entity)
        {
            EntityType = entityType;
            EntityId = entityId;
            Entity = entity;
            DateCreatedUTC = DateTime.UtcNow;
        }

        /// <summary>
        ///     Gets or sets the type of the entity, e.g., ToDoItem.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        ///     Gets or sets the entity Id.
        ///     Use this as the Partition Key, so that all the auditing records for the same entity are stored in the same logical partition.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        ///     Gets or sets the entity itself.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        ///     Gets or sets the date audit record create.
        /// </summary>
        public DateTime DateCreatedUTC { get; set; }
    }
}
