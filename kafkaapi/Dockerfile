FROM mcr.microsoft.com/dotnet/sdk:9.0.305-bookworm-slim AS build
RUN apt-get update && \
  apt-get install -y --no-install-recommends protobuf-compiler && \
  rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["kafkaapi.csproj", "kafkaapi/"]
RUN dotnet restore "kafkaapi/kafkaapi.csproj"
COPY . .
WORKDIR /src
RUN dotnet publish "kafkaapi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "kafkaapi.dll"]