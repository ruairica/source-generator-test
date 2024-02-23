using WorkerExample;
using DependencyGen;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddGeneratedDependencies();


builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();
