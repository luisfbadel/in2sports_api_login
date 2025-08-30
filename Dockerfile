# Etapa 1: Construcción de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de la solución y restaurar dependencias
COPY ["auth.in2sport.api/auth.in2sport.api.csproj", "auth.in2sport.api/"]
COPY ["auth.in2sport.application/auth.in2sport.application.csproj", "auth.in2sport.application/"]
COPY ["auth.in2sport.infrastructure/auth.in2sport.infrastructure.csproj", "auth.in2sport.infrastructure/"]
RUN dotnet restore "auth.in2sport.api/auth.in2sport.api.csproj"

# Copiar el resto del código y compilar la aplicación
COPY . .
WORKDIR "/src/auth.in2sport.api"
RUN dotnet publish -c Release -o /app

# Etapa 2: Imagen final con runtime de .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar los archivos compilados de la etapa anterior
COPY --from=build /app .

# Exponer el puerto en el que corre la API
EXPOSE 8080

# Configurar el entrypoint de la aplicación
ENTRYPOINT ["dotnet", "auth.in2sport.api.dll"]