using JobRadar.Infrastructure.Persistence;
using JobRadar.MigrationService;
using JobRadar.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<JobRadarDbContext>("jobradar");
builder.Services.AddHostedService<MigrationWorker>();

builder.Build().Run();
