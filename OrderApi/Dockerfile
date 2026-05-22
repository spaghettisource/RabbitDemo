FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY Contracts/Contracts.csproj Contracts/
COPY OrderApi/OrderApi.csproj OrderApi/

RUN dotnet restore OrderApi/OrderApi.csproj

COPY . .

WORKDIR /src/OrderApi

RUN dotnet publish OrderApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderApi.dll"]