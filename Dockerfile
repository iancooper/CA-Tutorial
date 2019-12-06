# Note that we set context from the docker-compose file to allow docker to have a context above the docker file

FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
WORKDIR /src/GreetingsApp/
RUN dotnet publish GreetingsApp.csproj -c Release -o /app


FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5000

#run the site
ENTRYPOINT ["dotnet", "GreetingsApp.dll"]
