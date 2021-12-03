using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class MigrationService : IHostedService
    {
        private readonly GoogleService _googleService;
        private readonly MigrationSettings _migrationSettings;
        private readonly IDbContextFactory<VideosContext> _videosContextFactory;
        private readonly IDbContextFactory<VideosMigrationContext> _videosMigrationContextFactory;
        public MigrationService(GoogleService googleService, IOptions<MigrationSettings> migrationSettings, IDbContextFactory<VideosContext> videosContextFactory, IDbContextFactory<VideosMigrationContext> videosMigrationContextFactory)
        {
            _googleService = googleService;
            _migrationSettings = migrationSettings.Value;
            _videosContextFactory = videosContextFactory;
            _videosMigrationContextFactory = videosMigrationContextFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if(_migrationSettings.MigrationString != null && _migrationSettings.ClipsPath != null)
            {
                var videosMigration = _videosMigrationContextFactory.CreateDbContext();
                var videos = _videosContextFactory.CreateDbContext();
                var tableEntries = videosMigration.VideoIds.AsQueryable().OrderBy(v => v.AddDate).ToArray();
                for (int i = 0; i < tableEntries.Length; i++)
                {
                    var stream = System.IO.File.OpenRead(_migrationSettings.ClipsPath + tableEntries[i].Id + ".mp4");
                    await _googleService.Upload(stream, tableEntries[i].Videoname, tableEntries[i].Userid);
                    Console.WriteLine(tableEntries[i].Videoname + " finished uploading.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
