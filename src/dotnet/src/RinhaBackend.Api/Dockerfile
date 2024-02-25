FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9999

ENV ASPNETCORE_URLS=http://+:9999

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/RinhaBackend.Api/RinhaBackend.Api.csproj", "src/RinhaBackend.Api/"]
RUN dotnet restore "src/RinhaBackend.Api/RinhaBackend.Api.csproj"
COPY . .
WORKDIR "/src/src/RinhaBackend.Api"
RUN dotnet build "RinhaBackend.Api.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "RinhaBackend.Api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RinhaBackend.Api.dll"]
