# ==============================================================================
# Dockerfile Multi-Stage Build para a Aplicação ONG Vestindo Sonhos
# Compatível com .NET 10 (ASP.NET Core Web MVC + REST API)
# ==============================================================================

# ------------------------------------------------------------------------------
# Estágio 1: Base de Runtime (Imagem enxuta e segura para execução)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Configura a URL de escuta padrão do ASP.NET Core em containers Linux
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# ------------------------------------------------------------------------------
# Estágio 2: SDK e Build (Restaura dependências e compila os binários)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o arquivo de definição de projeto para maximizar cache de camadas do Docker
COPY ["VestindoSonhos.csproj", "./"]
RUN dotnet restore "VestindoSonhos.csproj"

# Copia os demais arquivos da aplicação
COPY . .

# Compila o projeto no modo Release
RUN dotnet build "VestindoSonhos.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ------------------------------------------------------------------------------
# Estágio 3: Publicação (Gera os artefatos otimizados e prontos para produção)
# ------------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VestindoSonhos.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Estágio 4: Imagem Final de Produção (Apenas o runtime e binários publicados)
# ------------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

# Copia apenas os arquivos publicados do estágio anterior
COPY --from=publish /app/publish .

# Define o ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "VestindoSonhos.dll"]
