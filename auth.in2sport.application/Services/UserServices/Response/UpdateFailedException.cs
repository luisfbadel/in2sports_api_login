using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth.in2sport.application.Services.UserServices.Response
{
    public class UpdateFailedException : Exception
    {
        public UpdateFailedException() : base() { }

        public UpdateFailedException(string message) : base(message) { }

        public UpdateFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
