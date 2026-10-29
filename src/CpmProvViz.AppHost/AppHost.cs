var builder = DistributedApplication.CreateBuilder(args);

builder.AddJavaApp("provenance-service", "../../provenance-service")
    .WithMavenGoal("spring-boot:run")
    .WithHttpEndpoint(env: "SERVER_PORT")
    .WithHttpHealthCheck("/health");

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
