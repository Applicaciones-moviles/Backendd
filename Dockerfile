# Etapa 1: Build de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copiamos los archivos de solución y de proyecto
COPY *.sln .
COPY DriveSafe.Presentation/*.csproj ./DriveSafe.Presentation/
COPY DriveSafe.Application/*.csproj ./DriveSafe.Application/
COPY DriveSafe.Domain/*.csproj ./DriveSafe.Domain/
COPY DriveSafe.Infraestructure/*.csproj ./DriveSafe.Infraestructure/
COPY DriveSafe.Shared/*.csproj ./DriveSafe.Shared/

# Restaurar dependencias
RUN dotnet restore

# Copiamos el resto de los archivos del proyecto
COPY . .

# Construir el proyecto
RUN dotnet publish -c Release -o /app/out

# Etapa 2: Imagen de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Variables de entorno para la aplicación
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Copiamos los archivos de la etapa de build
COPY --from=build /app/out .

# Exponer el puerto donde la aplicación escuchará
EXPOSE 8080

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "DriveSafe.Presentation.dll"]
