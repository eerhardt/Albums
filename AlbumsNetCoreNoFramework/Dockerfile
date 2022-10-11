FROM alpine as base
RUN apk add libstdc++

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
RUN apk add clang lld musl-dev build-base zlib-dev krb5-dev

WORKDIR /src
COPY ["AlbumsNetCoreNoFramework.csproj", "."]
RUN dotnet restore "./AlbumsNetCoreNoFramework.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "AlbumsNetCoreNoFramework.csproj" -c Release -o /app/publish -r linux-musl-x64 --self-contained

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM base AS final
EXPOSE 5000
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["/app/AlbumsNetCoreNoFramework"]