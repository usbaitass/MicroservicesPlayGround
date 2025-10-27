FROM mcr.microsoft.com/dotnet/sdk:9.0.305-bookworm-slim AS build
RUN apt-get update && \
  apt-get install -y --no-install-recommends protobuf-compiler && \
  rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["websocketapi.csproj", "websocketapi/"]
RUN dotnet restore "websocketapi/websocketapi.csproj"
COPY . .
WORKDIR /src
RUN dotnet publish "websocketapi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "websocketapi.dll"]