using LoggingWebApi.Services;
using LoggingWebApi.Configuration;
using LoggingWebApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure LoggingOptions
builder.Services.Configure<LoggingOptions>(
    builder.Configuration.GetSection("LoggingOptions"));

// Register LoggingService
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<ILogEntrySaverFactory, LogEntrySaverFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Use our LoggingController for all routes
app.MapControllers();

app.Run();