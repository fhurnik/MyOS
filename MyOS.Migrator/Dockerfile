FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore MyOS.Migrator/MyOS.Migrator.csproj

RUN dotnet publish MyOS.Migrator/MyOS.Migrator.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MyOS.Migrator.dll"]