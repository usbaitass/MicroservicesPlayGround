# MicroservicesPlayGround
The project contains multiple microservice projects that interact with each other through different technologies.

![Alt text](https://drive.google.com/uc?export=view&id=1sbXvDDQ-KiyFJC1KVMUAsKIY1BeMDLwK)

### run projects in docker containers
```
docker compose build --no-cache
docker compose up
docker compose down
```

### Endpoints to use
```
WEB APP http://localhost:4202/
POST http://localhost:5228/messages
GET http://localhost:5228/messages
GraphQL http://localhost:5228/graphql/
```

### setup certificate
```
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p 123asd
dotnet dev-certs https --trust
```

# COMMANDS USE FOR REFERENCE

### add project to the solution
```
dotnet new web -n grpcapi
dotnet sln add grpcapi/grpcapi.csproj 
```

### setup rabbitmq docker container
```
docker run -d --hostname rabbit-host --name rabbitmq   -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
