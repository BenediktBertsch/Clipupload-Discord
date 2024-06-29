using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IO;
using System.Security.Cryptography;

namespace Backend.Services
{
    public class ConsistencyCheckService : IHostedService
    {
        private readonly IDbContextFactory<VideosContext> _videosContextFactory;
        private readonly FilesSettings _filesSettings;
        private readonly MD5 _md5alg;
        public ConsistencyCheckService(IDbContextFactory<VideosContext> videosContextFactory, IOptions<FilesSettings> filesSettings) 
        {
            _videosContextFactory = videosContextFactory;
            _filesSettings = filesSettings.Value;
            _md5alg = (MD5)CryptoConfig.CreateFromName("MD5");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Check from DB
            var videos = _videosContextFactory.CreateDbContext();
            var allVideos = videos.VideoIds;
            foreach (var video in allVideos)
            {
                string videoPath = Path.GetFullPath(_filesSettings.Path + Path.DirectorySeparatorChar + video.User.ToString() + Path.DirectorySeparatorChar + video.Id + ".mp4");
                string thumbnailPath = Path.GetFullPath(_filesSettings.Path + Path.DirectorySeparatorChar + video.User.ToString() + Path.DirectorySeparatorChar + video.Id + ".avif");
                if (!File.Exists(videoPath))
                    Console.WriteLine(videoPath);
                if (!File.Exists(thumbnailPath))
                    Console.WriteLine(thumbnailPath);
                if (!File.Exists(thumbnailPath) && File.Exists(videoPath))
                {
                    Console.WriteLine("Generating " + thumbnailPath);
                    Utils.GenerateThumbnail(videoPath, thumbnailPath);
                }
            }

            // Check from files
            foreach (string file in Directory.EnumerateFiles(_filesSettings.Path, "*.*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var found = allVideos.Any(video => video.Id == fileName);
                if (!found)
                {
                    Console.WriteLine(file + " is not in the DB, there are inconsistencies!");
                    var hash = Utils.GenerateHash(_md5alg, File.OpenRead(file));
                    Console.WriteLine("Hash of the file is: " + hash + ", may check the DB if its an duplicate.");
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
