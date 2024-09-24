using auth.in2sport.api.Middleware;
using auth.in2sport.application.Services.LoginServices;
using auth.in2sport.application.Services.UserServices;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Environment variable
var env = builder.Environment;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//Dependency Injection for services
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IUserService, UserService>();

//Dependency Injection for repositories
builder.Services.AddDbContext<PostgresDbContext>();
builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(PostgresRepository<>));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Autentication
var securityKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(securityKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.Use(async (context, next) =>
//{
//    if (context.Request.Method == "OPTIONS")
//    {
//        context.Response.Headers.Add("Access-Control-Allow-Origin", builder.Configuration["OriginUrl"]);
//        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
//        context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, X-API-KEY, Content-Type");
//        context.Response.StatusCode = 200;
//        return;
//    }

//    await next();
//});
// Registrar el middleware de API Key.
//app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();