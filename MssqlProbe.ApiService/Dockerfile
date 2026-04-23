FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MssqlProbe.ApiService/MssqlProbe.ApiService.csproj", "MssqlProbe.ApiService/"]
COPY ["AspireApp1.ServiceDefaults/AspireApp1.ServiceDefaults.csproj", "AspireApp1.ServiceDefaults/"]
RUN dotnet restore "MssqlProbe.ApiService/MssqlProbe.ApiService.csproj"
COPY . .
WORKDIR "/src/MssqlProbe.ApiService"
RUN dotnet build "./MssqlProbe.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MssqlProbe.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MssqlProbe.ApiService.dll"]
