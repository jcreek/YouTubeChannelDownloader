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
                string pathString = await DownloadVideoAsync(video, channel.Title);
                await RecordSuccessfulVideoDownload(video, channel.Title);

                // TODO? - set this up with a discard to run in a separate thread so more downloads can happen, need to be careful of error handling and logging
                TransferFileToNasShare(pathString);
                DeleteLocalFile(pathString);
            }
        }
        }
}
