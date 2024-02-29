using auth.in2sport.application.Services.LoginServices;
using auth.in2sport.application.Services.UserServices;
using auth.in2sport.infrastructure.Repositories;
using auth.in2sport.infrastructure.Repositories.Postgres;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Dependency Injection for services
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IUserService, UserService>();


//Dependency Injection for repositories
builder.Services.AddDbContext<PostgresDbContext>();
builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(PostgresRepository<>));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(Options =>
{
    Options.AddPolicy("ApiPolitics", app =>
    {
        app.AllowAnyOrigin()
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
app.UseHttpsRedirection();
app.UseCors("ApiPolitics");
app.UseAuthorization();
app.MapControllers();
app.Run();