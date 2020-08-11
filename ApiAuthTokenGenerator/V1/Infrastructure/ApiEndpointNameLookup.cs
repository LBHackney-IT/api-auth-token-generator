using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.Infrastructure
{
    [Table("api_endpoint_lookup")]
    public class ApiEndpointNameLookup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
        [Column("api_endpoint_name")]
        public string ApiEndpointName { get; set; }
        [Column("api_lookup_id")]
        [ForeignKey("api_lookup")]
        public int ApiLookupId { get; set; }
    }
}