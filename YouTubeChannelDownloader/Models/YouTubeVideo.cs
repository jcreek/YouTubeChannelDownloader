using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;

namespace YouTubeChannelDownloader.Models
{
    public class YouTubeVideo
    {
        public PlaylistVideo PlaylistVideo { get; set; }
        public int VideoNumber { get; set; }
    }
}
