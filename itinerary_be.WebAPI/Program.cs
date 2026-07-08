using itinerary_be.Core.Domain.Enums;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Itinerary;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<ItineraryDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.MapEnum<EventTypes>("event_type", "itinerary")));

// Add Trip services
builder.Services.AddTripServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // OpenAPI features can be added here 

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();