using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Chents.API.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using System.Reflection;
using Chents_GS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Oracle Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// RabbitMQ Configuration
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService<AlertConsumerService>();

// ML.NET Service
builder.Services.AddSingleton<FloodPredictionService>();

// Hangfire for Rate Limiting
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chents API", Version = "v1" });
    
    // Include XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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

// Hangfire Dashboard
app.UseHangfireDashboard();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();