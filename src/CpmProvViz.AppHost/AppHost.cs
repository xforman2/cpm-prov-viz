var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.CpmProvViz_Api>("api")
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", url =>
    {
        url.Url = "/swagger";
        url.DisplayText = "Swagger UI";
    });

builder.AddViteApp("frontend", "../../frontend")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
