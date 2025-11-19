# Etapa base: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Etapa build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos *todo* el repo (desde la raíz)
COPY . .

# Restaurar y publicar usando la ruta al csproj
RUN dotnet restore "Auth.Api/Auth.Api.csproj"
RUN dotnet publish "Auth.Api/Auth.Api.csproj" -c Release -o /app/out

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Asegúrate que el dll se llame así (revísalo en bin/Release/netX.X/)
ENTRYPOINT ["dotnet", "Auth.Api.dll"]