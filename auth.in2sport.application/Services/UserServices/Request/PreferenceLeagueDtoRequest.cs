using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using MercadoPago.Resource.User;

namespace auth.in2sport.application.Services.UserServices.Request
{
    public class PreferenceLeagueDtoRequest
    {
        public string Title { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public List<LeagueUser>? Data { get; set; }
    }

    public class LeagueUser
    {
        public Users? User { get; set; }

        public List<string>? Courses { get; set; }  

        public int Price { get; set; }
    }
}
