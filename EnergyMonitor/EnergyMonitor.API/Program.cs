using EnergyMonitor.API;
using EnergyMonitor.API.Interface;
using EnergyMonitor.API.Repository;
using EnergyMonitor.API.Services;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7051")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Services
builder.Services.AddControllers();
builder.Services.AddSingleton<DBContext>();
builder.Services.AddSingleton<IEnergyRepository, EnergyRepository>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddHostedService<MqttSubscriberService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowMVC");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
