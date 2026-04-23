FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AspireApp1.ApiService/AspireApp1.ApiService.csproj", "AspireApp1.ApiService/"]
COPY ["AspireApp1.ServiceDefaults/AspireApp1.ServiceDefaults.csproj", "AspireApp1.ServiceDefaults/"]
RUN dotnet restore "AspireApp1.ApiService/AspireApp1.ApiService.csproj"
COPY . .
WORKDIR "/src/AspireApp1.ApiService"
RUN dotnet build "./AspireApp1.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AspireApp1.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspireApp1.ApiService.dll"]
