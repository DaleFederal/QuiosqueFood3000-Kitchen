# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files
COPY *.sln ./
COPY QuiosqueFood3000.Kitchen.Api/*.csproj ./QuiosqueFood3000.Kitchen.Api/
COPY QuiosqueFood3000.Kitchen.Application/*.csproj ./QuiosqueFood3000.Kitchen.Application/
COPY QuiosqueFood3000.Kitchen.Domain/*.csproj ./QuiosqueFood3000.Kitchen.Domain/
COPY QuiosqueFood3000.Kitchen.Infrastructure/*.csproj ./QuiosqueFood3000.Kitchen.Infrastructure/
COPY QuiosqueFood3000.Kitchen.Tests/*.csproj ./QuiosqueFood3000.Kitchen.Tests/

# Restore dependencies with verbose logging and clear cache
RUN dotnet nuget locals all --clear
RUN dotnet restore --verbosity normal

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the API project
RUN dotnet publish QuiosqueFood3000.Kitchen.Api/QuiosqueFood3000.Kitchen.Api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "QuiosqueFood3000.Kitchen.Api.dll"]
