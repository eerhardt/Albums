FROM alpine as base
RUN apk add libstdc++

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
ARG SERVER_GC=true
RUN apk add clang lld musl-dev build-base zlib-dev krb5-dev

WORKDIR /src
COPY ["AlbumsNetCore.csproj", "."]
RUN dotnet restore "./AlbumsNetCore.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "AlbumsNetCore.csproj" -c Release -o /app/publish -r linux-musl-x64 --self-contained -p:ServerGarbageCollection=${SERVER_GC}

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM base AS final
ENV DOTNET_URLS=http://+:5000
EXPOSE 5000
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["/app/AlbumsNetCore"]