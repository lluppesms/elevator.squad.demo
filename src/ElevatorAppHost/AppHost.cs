var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ElevatorApi>("elevator-api");

builder.Build().Run();
