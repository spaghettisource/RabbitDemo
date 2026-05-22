using OrderWorker;
using RabbitDemo.Data.Repositories;

var builder = Host.CreateApplicationBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("TicketDb")
    ?? throw new InvalidOperationException(
        "Connection string TicketDb not found.");

builder.Services.AddSingleton<ITicketRepository>(
    _ => new TicketRepository(connectionString));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();