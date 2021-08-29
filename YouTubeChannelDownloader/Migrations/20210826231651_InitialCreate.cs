using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeChannelDownloader.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadedVideos",
                columns: table => new
                {
                    DownloadedVideoId = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadedVideos", x => x.DownloadedVideoId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadedVideos");
        }
    }
}
