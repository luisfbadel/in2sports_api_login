using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class Application : PostgresEntity
    {
        public required string Name { get; set; }
        public Organization? Organization { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
