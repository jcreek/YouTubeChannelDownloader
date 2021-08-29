using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using YouTubeChannelDownloader.Models;

namespace YouTubeChannelDownloader.Data
{
    public class DownloadedVideosContext : DbContext
    {
        public DbSet<DownloadedVideo> DownloadedVideos { get; set; }

        public string DbPath { get; private set; }

        public DownloadedVideosContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}db{System.IO.Path.DirectorySeparatorChar}downloadedvideos.sqlite";
            Console.WriteLine($"db path is {DbPath}");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
