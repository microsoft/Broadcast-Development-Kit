using System.Runtime.Serialization;

namespace Application.Common.Models
{
    /// <summary>
    /// Join URL context.
    /// </summary>
    [DataContract]
    public class JoinUrlContext
    {
        /// <summary>
        /// Gets or sets the Tenant Id.
        /// </summary>
        [DataMember]
        public string Tid { get; set; }

        /// <summary>
        /// Gets or sets the AAD object id of the user.
        /// </summary>
        [DataMember]
        public string Oid { get; set; }

        /// <summary>
        /// Gets or sets the chat message id.
        /// </summary>
        [DataMember]
        public string MessageId { get; set; }
    }
}
