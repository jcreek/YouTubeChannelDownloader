using Creek.HelpfulExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeChannelDownloader.Data;
using YouTubeChannelDownloader.Models;
using YouTubeChannelDownloader.Services;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.Streams;
using YouTubeChannelDownloader.Utilities;
using System.IO;

namespace YouTubeChannelDownloader
{
    internal class Program
    {
        #region Private Variables

        private static DownloadedVideosContext _db = new DownloadedVideosContext();
        private static string _channelId = Environment.GetEnvironmentVariable("CHANNEL_ID", EnvironmentVariableTarget.Process);
        private static YoutubeClient _youtube = new YoutubeClient();

        #endregion Private Variables

        private static async Task Main(string[] args)
        {
            // Check every night if there's any new videos on the channel and download them
            // Could change the interval and channel via a config file or environment variables
            IReadOnlyList<YouTubeVideo> newVideos = await CheckForNewVideosAsync();

            YoutubeExplode.Channels.Channel channel = await _youtube.Channels.GetAsync(_channelId);

            Console.WriteLine($"There are {newVideos.Count} new videos to download from {channel.Title}");

            foreach (YouTubeVideo video in newVideos)
            {
                Console.WriteLine($"Starting {channel.Title} - {video.PlaylistVideo.Title}");
                try
                {
                    string pathString = await DownloadVideoAsync(video, channel.Title);
                    await RecordSuccessfulVideoDownload(video, channel.Title);

                    // TODO? - set this up with a discard to run in a separate thread so more downloads can happen, need to be careful of error handling and logging
                    TransferFileToNasShare(pathString);
                    DeleteLocalFile(pathString);
                }
                catch (Exception ex)
                {
                    // Wait a few minutes then try again
                    int minutesToWait = 2;
                    await Task.Delay(minutesToWait * 60 * 1000);

                    try
                    {
                        string pathString = await DownloadVideoAsync(video, channel.Title);
                        await RecordSuccessfulVideoDownload(video, channel.Title);

                        // TODO? - set this up with a discard to run in a separate thread so more downloads can happen, need to be careful of error handling and logging
                        TransferFileToNasShare(pathString);
                        DeleteLocalFile(pathString);
                    }
                    catch (Exception ex2)
                    {
                        // Give up on this video, and log the error
                        Console.WriteLine($"Unable to process video after retrying due to error: {ex2.Message}");
                    }
                }
            }
        }

        private static async Task<List<YouTubeVideo>> CheckForNewVideosAsync()
        {
            // Get the current videos on the channel
            IReadOnlyList<PlaylistVideo> videos = await _youtube.Channels.GetUploadsAsync(_channelId);

            List<YouTubeVideo> indexedVideos = new List<YouTubeVideo>();

            foreach (var (video, index) in videos.Reverse().WithIndex())
            {
                YouTubeVideo youtubeVideo = new YouTubeVideo()
                {
                    PlaylistVideo = video,
                    VideoNumber = index + 1, // These are zero-indexed but for episode counts we want 1 indexing
                };

                indexedVideos.Add(youtubeVideo);
            }

            List<string> downloadedVideoIds = _db.DownloadedVideos.Select(dv => dv.DownloadedVideoId).ToList();

            // Make a new list of only the videos we've not alread downloaded
            List<YouTubeVideo> newVideos = indexedVideos.Where(v => !downloadedVideoIds.Contains(v.PlaylistVideo.Id)).ToList();

            return newVideos;
        }

        private static async Task<string> DownloadVideoAsync(YouTubeVideo youtubeVideo, string channelTitle)
        {
            PlaylistVideo video = youtubeVideo.PlaylistVideo;

            // Tequest the manifest that lists all available streams for a particular video
            StreamManifest streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

            // Filter through the streams and select the video and audio separately to get videos above 720p30
            // Highest bitrate audio-only stream
            IStreamInfo audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // Highest quality MP4 video-only stream
            IVideoStreamInfo videoStreamInfo = streamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestVideoQuality();

            // Put them together into a new stream collection
            IStreamInfo[] streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

            // Generate a valid filename
            string origFileName = $"{channelTitle} S01E{youtubeVideo.VideoNumber} - {video.Title}.{videoStreamInfo.Container.Name}";
            string fileName = MakeValidFileName(origFileName);

            Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            string path = Environment.GetFolderPath(folder);
            string pathString = $"{path}{System.IO.Path.DirectorySeparatorChar}{fileName}";

            string dockerFfmpegPath = "/usr/bin/ffmpeg";

            // Windows
            //ConversionRequest request = new ConversionRequestBuilder(pathString).Build();

            // Docker
            ConversionRequest request = new ConversionRequestBuilder(pathString).SetFFmpegPath(dockerFfmpegPath).Build();

            // Display progress in console
            using (InlineProgress progress = new InlineProgress())
            {
                // Download and process them into one file
                await _youtube.Videos.DownloadAsync(streamInfos, request, progress);
            }

            Console.WriteLine($"Stored video at {pathString}");

            return pathString;
        }

        private static void TransferFileToNasShare(string pathString)
        {
            SftpConfig sftpConfig = new SftpConfig()
            {
                Host = Environment.GetEnvironmentVariable("SFTP_HOST", EnvironmentVariableTarget.Process),
                Port = int.Parse(Environment.GetEnvironmentVariable("SFTP_PORT", EnvironmentVariableTarget.Process)),
                UserName = Environment.GetEnvironmentVariable("SFTP_USER", EnvironmentVariableTarget.Process),
                Password = Environment.GetEnvironmentVariable("SFTP_PASSWORD", EnvironmentVariableTarget.Process),
            };

            Console.WriteLine($"Path: {pathString}");

            string origFileName = RemoveSlashesFromString(pathString);
            string fileName = MakeValidFileName(origFileName);

            Console.WriteLine($"Filename: {fileName}");

            SftpService sftpService = new SftpService(null, sftpConfig);
            string destinationFolder = Environment.GetEnvironmentVariable("SFTP_DESTINATIONFOLDER", EnvironmentVariableTarget.Process);
            sftpService.UploadFile(pathString, destinationFolder, fileName);

            Console.WriteLine("SFTP done");
        }

        private static void DeleteLocalFile(string pathString)
        {
            try
            {
                // Check if file exists with its full path
                if (File.Exists(pathString))
                {
                    // If file found, delete it
                    File.Delete(pathString);
                    Console.WriteLine("Local file deleted.");
                }
                else Console.WriteLine("Local file not found");
            }
            catch (IOException ioExp)
            {
                Console.WriteLine(ioExp.Message);
            }
        }

        private static async Task RecordSuccessfulVideoDownload(YouTubeVideo youtubeVideo, string channelTitle)
        {
            PlaylistVideo video = youtubeVideo.PlaylistVideo;

            DownloadedVideo downloadedVideo = new DownloadedVideo()
            {
                DownloadedVideoId = video.Id,
                DownloadedDateTime = DateTime.Now,
                Title = $"{channelTitle} {youtubeVideo.VideoNumber} - {video.Title}",
            };

            await _db.AddAsync(downloadedVideo);
            await _db.SaveChangesAsync();

            Console.WriteLine($"Recorded download in db of '{channelTitle} {youtubeVideo.VideoNumber} - {video.Title}'");
        }

        private static string MakeValidFileName(string originalFileName)
        {
            char[] invalids = System.IO.Path.GetInvalidFileNameChars();
            string validFileName = String.Join("_", originalFileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // Additionally remove Windows characters explicitly, in case we're running on Linux (e.g. in Docker) but need to access the files in Windows
            string[] charsToRemove = new string[] { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };

            validFileName = RemoveCharsFromString(charsToRemove, validFileName);

            return validFileName;
        }

        private static string RemoveSlashesFromString(string str)
        {
            string[] charsToRemove = new string[] { "/", @"\" };

            return RemoveCharsFromString(charsToRemove, str);
        }

        private static string RemoveCharsFromString(string[] charsToRemove, string str)
        {
            foreach (string c in charsToRemove)
            {
                str = str.Replace(c, string.Empty);
            }

            return str;
        }
    }
}
