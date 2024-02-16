ARG CURL=with
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled as base
WORKDIR /app
ENV ASPNETCORE_URLS http://*:9999
ENTRYPOINT ["/app/RinhaBackend.Api"]

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
RUN apt-get update && apt-get install clang zlib1g-dev curl -y --no-install-recommends
RUN cd /; ldd /usr/bin/curl | tr -s '[:blank:]' '\n' | grep '^/' \
    | sort | uniq \
    | xargs -I % sh -c 'mkdir -p $(dirname deps%); cp % deps%;'
WORKDIR /app
COPY ./src/RinhaBackend.Api/RinhaBackend.Api.csproj ./
RUN dotnet restore -r linux-x64 -p:Configuration=Release
COPY . .
RUN dotnet publish --no-restore -o out

FROM base
COPY --from=build /app/out .

