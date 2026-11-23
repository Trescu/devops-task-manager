FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY src/TaskManager.Api/TaskManager.Api.csproj src/TaskManager.Api/
RUN dotnet restore src/TaskManager.Api/TaskManager.Api.csproj
COPY src/TaskManager.Api/ src/TaskManager.Api/
RUN dotnet publish src/TaskManager.Api/TaskManager.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
