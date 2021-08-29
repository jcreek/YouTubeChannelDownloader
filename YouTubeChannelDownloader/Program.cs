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
        }
}
