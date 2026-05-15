FROM mcr.microsoft.com/dotnet/sdk:10.0.200 AS build
WORKDIR /src

COPY backend/src/FrangoFrito.Domain/FrangoFrito.Domain.csproj backend/src/FrangoFrito.Domain/
COPY backend/src/FrangoFrito.Application/FrangoFrito.Application.csproj backend/src/FrangoFrito.Application/
COPY backend/src/FrangoFrito.Infrastructure/FrangoFrito.Infrastructure.csproj backend/src/FrangoFrito.Infrastructure/
COPY backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj backend/src/FrangoFrito.Api/

RUN dotnet restore backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj

COPY backend/src backend/src

RUN dotnet publish backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FrangoFrito.Api.dll"]
