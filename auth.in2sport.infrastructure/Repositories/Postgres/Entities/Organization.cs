using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
    public class Organization : PostgresEntity
    {
        public string Name { get; set; }
    }
}
