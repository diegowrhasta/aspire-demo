FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AspireApp1.RabbitConsumer/AspireApp1.RabbitConsumer.csproj", "AspireApp1.RabbitConsumer/"]
COPY ["AspireApp1.ServiceDefaults/AspireApp1.ServiceDefaults.csproj", "AspireApp1.ServiceDefaults/"]
RUN dotnet restore "AspireApp1.RabbitConsumer/AspireApp1.RabbitConsumer.csproj"
COPY . .
WORKDIR "/src/AspireApp1.RabbitConsumer"
RUN dotnet build "./AspireApp1.RabbitConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AspireApp1.RabbitConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspireApp1.RabbitConsumer.dll"]
