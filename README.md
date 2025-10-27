# MicroservicesPlayGround
The project contains multiple microservice projects that interact with each other through different technologies.


### run projects in docker containers
```
docker compose up
docker compose build --no-cache
docker compose down
```

### setup certificate
```
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p 123asd
dotnet dev-certs https --trust
```

### add project to the solution
```
dotnet sln add grpcapi/grpcapi.csproj 
```