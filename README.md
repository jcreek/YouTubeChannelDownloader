# YouTubeChannelDownloader

This Dockerized application is intended to enable programmatic downloading of all videos on a given YouTube channel, and upload them to an SFTP server, ready for use with Plex. My personal use case for this is to upload the files to a Handbrake Docker container before then importing the converted files to Plex.

## Running on Windows instead of Docker

This can be run on Windows instead of Docker, however you will have to make some modifications.

Uncomment the two `FFmpeg` targets in the csproj file, this will enable downloading FFmpeg using the ps1 script on build.

In `DownloadVideoAsync` uncomment the Windows `ConversionRequest` and comment out the Docker one. This enables using the FFmpeg from the project folder, rather than one at a specific Linux path for Docker.

Set the channel id manually in the `Private Variables` rather than reading from an environment variable - this should be read from a config file, or you can manually specify environment variables in Visual Studio itself if you don't want to make code changes.

Set the SFTP details manually in `TransferFileToNasShare` rather than reading from environment variables - this should be read from a config file, or you can manually specify environment variables in Visual Studio itself if you don't want to make code changes.

## Build the database

To build the database, open a terminal from the project folder (not the solution folder) and run each of these commands in turn.

`dotnet tool install --global dotnet-ef`
`dotnet add package Microsoft.EntityFrameworkCore.Design`
`dotnet ef migrations add InitialCreate`
`dotnet ef database update`

On Windows this will create a database file for you in the folder `C:\Users\YOURUSERNAME\AppData\Local`.
