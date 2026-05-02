#:sdk Aspire.AppHost.Sdk@13.2.4
#:package Aspire.Hosting.PostgreSQL@13.2.4

#pragma warning disable ASPIRECSHARPAPPS001

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb();

var rocketRepsDb = postgres.AddDatabase("rocketrepsdb", "rocketreps");

builder.AddCSharpApp("web", "./RocketReps.Web/RocketReps.Web.csproj", options =>
{
    options.LaunchProfileName = "https";
})
    .WithReference(rocketRepsDb)
    .WaitFor(rocketRepsDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();
