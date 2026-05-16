# Base image for running the app (NET 8)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Image for building the app (NET 8 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FloraAI.API/FloraAI.API.csproj", "FloraAI.API/"]
RUN dotnet restore "FloraAI.API/FloraAI.API.csproj"
COPY . .
WORKDIR "/src/FloraAI.API"
RUN dotnet build "FloraAI.API.csproj" -c Release -o /app/build

# Publishing the app
FROM build AS publish
RUN dotnet publish "FloraAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FloraAI.API.dll"]
