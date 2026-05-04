# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0 AS build
ARG PROJECT=RocketReps.Web/RocketReps.Web.csproj
WORKDIR /src

COPY . .
RUN dotnet restore ${PROJECT}

RUN dotnet publish ${PROJECT} -c Release -o /out \
    -p:PublishTrimmed=false -p:PublishReadyToRun=false

# ---------- Run (distroless) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0-distroless
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
WORKDIR /app
COPY --from=build /out ./

EXPOSE 8080
ENTRYPOINT ["./RocketReps.Web"]
