using CryptoJackpot.Identity.Application;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Data.Repositories;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Infra.IoC;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration);

// Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Application Layer (MediatR, Services, Handlers)
builder.Services.AddIdentityApplication();

// Register Infra.IoC Dependency Container (Bus)
DependencyContainer.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();