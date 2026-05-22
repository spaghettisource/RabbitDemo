FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY Contracts/Contracts.csproj Contracts/
COPY Data/Data.csproj Data/
COPY OrderWorker/OrderWorker.csproj OrderWorker/

RUN dotnet restore OrderWorker/OrderWorker.csproj

COPY . .

WORKDIR /src/OrderWorker

RUN dotnet publish OrderWorker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderWorker.dll"]