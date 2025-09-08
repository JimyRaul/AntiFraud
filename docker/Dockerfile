# docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["Antifraud.sln", "."]

# Copy project files
COPY ["src/Antifraud.Domain/Antifraud.Domain.csproj", "src/Antifraud.Domain/"]
COPY ["src/Antifraud.Application/Antifraud.Application.csproj", "src/Antifraud.Application/"]
COPY ["src/Antifraud.Infrastructure/Antifraud.Infrastructure.csproj", "src/Antifraud.Infrastructure/"]
COPY ["src/Antifraud.Api/Antifraud.Api.csproj", "src/Antifraud.Api/"]

# Restore dependencies
RUN dotnet restore "src/Antifraud.Api/Antifraud.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Antifraud.Api"
RUN dotnet build "Antifraud.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Antifraud.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Antifraud.Api.dll"]