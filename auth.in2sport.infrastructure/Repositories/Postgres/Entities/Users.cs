using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.infrastructure.Repositories.Postgres.Entities
{
   public class Users : PostgresEntity
    {
        public string? Email { get; set; }
        public byte[]? Password { get; set; }
        public int status { get; set; }
        public Guid Id { get; set; }
    }
}
