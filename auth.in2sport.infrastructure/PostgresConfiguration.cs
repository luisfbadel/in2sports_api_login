using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.infrastructure
{
    public class PostgresConfiguration
    {
        public required string AuthDbConnection { get; set; }
        public required string AuthDbDatabase { get; set; }
    }
}
