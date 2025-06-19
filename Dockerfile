# ----------------- Build Stage -----------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj dan restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy semua source code dan publish
COPY . ./
RUN dotnet publish -c Release -o /out

# ----------------- Runtime Stage -----------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "WashUpAPI.dll"]
