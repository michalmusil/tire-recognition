# Bring OpenCVSharp dependencies
FROM michalmusil/dotnet10-noble-opencv as opencv-builder

# BUILD APP
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /
COPY ["src/TireRecognition.WebApi/TireRecognition.WebApi.csproj", "src/TireRecognition.WebApi/"]
COPY ["src/TireRecognition.Application/TireRecognition.Application.csproj", "src/TireRecognition.Application/"]
COPY ["src/TireRecognition.Domain/TireRecognition.Domain.csproj", "src/TireRecognition.Domain/"]
COPY ["src/TireRecognition.Infrastructure/TireRecognition.Infrastructure.csproj", "src/TireRecognition.Infrastructure/"]
COPY ["src/TireRecognition.Shared/TireRecognition.Shared.csproj", "src/TireRecognition.Shared/"]
RUN dotnet restore "src/TireRecognition.WebApi/TireRecognition.WebApi.csproj"
COPY . .
WORKDIR "/src/TireRecognition.WebApi"
RUN dotnet build "TireRecognition.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TireRecognition.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt-get update
RUN apt-get install -y libc6 libicu-dev libfontconfig1
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/runtimes/linux-x64/native/libSkiaSharp.so /usr/lib/
COPY --from=opencv-builder /usr/lib /usr/lib
# COPY --from=skiasharp-builder out/linux/x64/ /usr/lib

EXPOSE 8080
ENV DOTNET_VERSION=10.0 ASPNET_VERSION=10.0 APP_UID=1654 ASPNETCORE_HTTP_PORTS=8080 DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "TireRecognition.WebApi.dll"]
