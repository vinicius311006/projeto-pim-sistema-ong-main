# =====================================================================
# Dockerfile Multi-Stage - ONG Vestindo Sonhos (PIM IV - UNIP)
# Etapa 8: Infraestrutura em Nuvem e DevOps (ASP.NET Core .NET 10)
# =====================================================================

# Estágio 1: Build e Publicação (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências NuGet
COPY ["VestindoSonhos/VestindoSonhos.csproj", "VestindoSonhos/"]
RUN dotnet restore "VestindoSonhos/VestindoSonhos.csproj"

# Copia todo o código-fonte e publica em modo Release
COPY . .
WORKDIR "/src/VestindoSonhos"
RUN dotnet publish "VestindoSonhos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime leve de Produção (ASP.NET Core 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VestindoSonhos.dll"]
