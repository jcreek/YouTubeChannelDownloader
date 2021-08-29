FROM mcr.microsoft.com/dotnet/runtime:5.0-focal AS base

# Install FFmpeg
RUN apt-get -y update
RUN apt-get -y upgrade
RUN apt-get install -y ffmpeg

# Copy fresh db to root
COPY ["YouTubeChannelDownloader/db/downloadedvideos.sqlite", "db/"]


FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["YouTubeChannelDownloader/YouTubeChannelDownloader.csproj", "YouTubeChannelDownloader/"]
RUN dotnet restore "YouTubeChannelDownloader/YouTubeChannelDownloader.csproj" --disable-parallel
COPY . .
WORKDIR "/src/YouTubeChannelDownloader"
RUN dotnet build "YouTubeChannelDownloader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YouTubeChannelDownloader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YouTubeChannelDownloader.dll"]
