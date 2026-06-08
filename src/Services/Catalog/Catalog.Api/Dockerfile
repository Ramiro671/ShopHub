FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/"]
COPY ["src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Services/Catalog/Catalog.Domain/"]
COPY ["src/Services/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Services/Catalog/Catalog.Application/"]
COPY ["src/Services/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Services/Catalog/Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Services/Catalog/Catalog.Api/"]
RUN dotnet restore "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Catalog/Catalog.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
