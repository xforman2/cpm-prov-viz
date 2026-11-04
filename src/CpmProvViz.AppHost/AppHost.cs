var builder = DistributedApplication.CreateBuilder(args);

var provenanceService = builder.AddJavaApp("provenance-service", "../../provenance-service")
    .WithMavenGoal("spring-boot:run")
    .WithHttpEndpoint(env: "SERVER_PORT")
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", url =>
    {
        url.Url = "/health";
        url.DisplayText = "Health";
    });

// Nothing uses the C# API now: the frontend reads documents through the provenance service. It stays
// registered and running until it is removed.
builder.AddProject<Projects.CpmProvViz_Api>("api")
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", url =>
    {
        url.Url = "/swagger";
        url.DisplayText = "Swagger UI";
    });

builder.AddViteApp("frontend", "../../frontend")
    .WithReference(provenanceService)
    .WaitFor(provenanceService);

builder.Build().Run();
