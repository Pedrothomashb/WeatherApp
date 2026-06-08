FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WeatherApp.Api/WeatherApp.Api.csproj", "WeatherApp.Api/"]
RUN dotnet restore "WeatherApp.Api/WeatherApp.Api.csproj"
COPY . .
WORKDIR "/src/WeatherApp.Api"
RUN dotnet build "WeatherApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "WeatherApp.Api.dll"]
