#:sdk Aspire.AppHost.Sdk@13.2.4
#:package Aspire.Hosting.PostgreSQL@13.2.4

#pragma warning disable ASPIRECSHARPAPPS001

var builder = DistributedApplication.CreateBuilder(args);

var rocketRepsDb = builder.AddPostgres("postgres")
    .AddDatabase("rocketrepsdb", "rocketreps");

builder.AddCSharpApp("web", "./RocketReps.Web/RocketReps.Web.csproj")
    .WithReference(rocketRepsDb)
    .WaitFor(rocketRepsDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();
