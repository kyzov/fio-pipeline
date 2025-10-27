var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.FIOpipeline_ApiService>("apiservice")
    .WithHttpEndpoint(port: 5000, name: "http-endpoint")
    .WithHttpsEndpoint(port: 5001, name: "https-endpoint")
    .WithHttpHealthCheck("/health");


builder.Build().Run();
