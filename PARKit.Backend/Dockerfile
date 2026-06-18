# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y restauramos dependencias
COPY ["PARKit.Backend.csproj", "./"]
RUN dotnet restore "./PARKit.Backend.csproj"

# Copiamos el resto del código y compilamos
COPY . .
RUN dotnet publish "PARKit.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponemos el puerto estándar 8080 para .NET 8
EXPOSE 8080

ENTRYPOINT ["dotnet", "PARKit.Backend.dll"]
