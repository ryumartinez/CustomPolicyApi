var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.CustomPolicyApi_ApiService>("apiservice");

builder.AddProject<Projects.CustomPolicyApi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
