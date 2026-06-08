FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/"]
COPY ["src/Services/Payment/Payment.Worker/Payment.Worker.csproj", "src/Services/Payment/Payment.Worker/"]
RUN dotnet restore "src/Services/Payment/Payment.Worker/Payment.Worker.csproj"
COPY . .
WORKDIR "/src/src/Services/Payment/Payment.Worker"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payment.Worker.dll"]
