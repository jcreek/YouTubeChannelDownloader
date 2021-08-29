using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeChannelDownloader.Models
{
    public class DownloadedVideo
    {
        [Key]
        public string DownloadedVideoId { get; set; }

        public DateTime DownloadedDateTime { get; set; }
        public string Title { get; set; }
    }
}
