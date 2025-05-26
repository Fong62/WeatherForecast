FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WeatherForecast/WeatherForecast.csproj ./WeatherForecast/ 
RUN dotnet restore ./WeatherForecast/WeatherForecast.csproj
COPY . .
RUN dotnet build ./WeatherForecast/WeatherForecast.csproj -c Release

FROM build AS publish
RUN dotnet publish ./WeatherForecast/WeatherForecast.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherForecast.dll"]