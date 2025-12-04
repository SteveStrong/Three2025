# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Three2025.csproj", "."]
RUN dotnet restore "Three2025.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Three2025.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Three2025.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published output
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Three2025.dll"]


# Build the image
# docker build -t three2025 .

# Run the container
# docker run -d -p 8080:8080 --name three2025-app three2025