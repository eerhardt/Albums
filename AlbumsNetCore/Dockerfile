FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build
RUN apk add clang lld musl-dev build-base zlib-dev krb5-dev

WORKDIR /src
COPY ["AlbumsNetCore.csproj", "."]
COPY ["nuget.config", "."]
RUN dotnet restore "./AlbumsNetCore.csproj"
COPY . .
WORKDIR "/src/."
ARG SERVER_GC=true
ARG BUILD_ARGS=
RUN dotnet publish "AlbumsNetCore.csproj" -c Release -o /app/publish -r linux-musl-x64 --self-contained -p:ServerGarbageCollection=${SERVER_GC} ${BUILD_ARGS}

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
ENV DOTNET_URLS=http://+:5000
EXPOSE 5000
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["/app/AlbumsNetCore"]