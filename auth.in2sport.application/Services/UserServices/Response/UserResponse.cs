using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.application.Services.UserServices.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public int status { get; set; }
    }
}
