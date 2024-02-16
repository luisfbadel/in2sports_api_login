namespace auth.in2sport.infrastructure
{
    public class PostgresConfiguration
    {
        public required string AuthDbConnection { get; set; }
        public required string AuthDbDatabase { get; set; }
    }
}
