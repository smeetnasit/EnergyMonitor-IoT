using EnergyMonitor.Simulation;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<MqttPublisher>();
builder.Services.AddSingleton<DBContext>();
builder.Services.AddSingleton<MachineLoader>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
