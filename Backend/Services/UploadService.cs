using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Models;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class UploadService
    {
        private ulong _userId;
        private DriveService _driveService;
        private AppSettings _appSettings;
        private DiscordService _discordService;
        private IDbContextFactory<VideosContext> _videosContextFactory;
        public void UploadStart(ulong userId, FilesResource.CreateMediaUpload request, DiscordService discordService, DriveService driveService, AppSettings appSettings, IDbContextFactory<VideosContext> videosContextFactory)
        {
            _discordService = discordService;
            _appSettings = appSettings;
            _videosContextFactory = videosContextFactory;
            _userId = userId;
            _driveService = driveService;
            request.ResponseReceived += UploadFinished;
            request.Upload();
        }

        private void UploadFinished(File file)
        {
            // Change Permissions, so that everyone can watch the video
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            var req = _driveService.Permissions.Create(permission, file.Id);
            req.SupportsAllDrives = true;
            req.SupportsTeamDrives = true;
            req.Execute();
            string videoId = "";
            string thumbnailLink = "";

            // Check if the video is processed
            Task.Run(() =>
            {
                // Get the uploaded file for createdtime attribute
                var _reqfile = _driveService.Files.Get(file.Id);
                _reqfile.SupportsAllDrives = true;
                _reqfile.SupportsTeamDrives = true;
                _reqfile.Fields = "createdTime,hasThumbnail,id,thumbnailLink";
                var _file = _reqfile.Execute();

                var processing = true;
                while (processing)
                {
                    Thread.Sleep(60000);
                    _reqfile = _driveService.Files.Get(file.Id);
                    _reqfile.SupportsAllDrives = true;
                    _reqfile.SupportsTeamDrives = true;
                    _reqfile.Fields = "createdTime,hasThumbnail,id,thumbnailLink";
                    _file = _reqfile.Execute();
                    if (_file.HasThumbnail == true)
                    {
                        processing = false;
                        videoId = _file.Id;
                        thumbnailLink = _file.ThumbnailLink;
                    }
                }

                // Add to db
                var id = Utils.GenerateId(_videosContextFactory);
                var _videosContext = _videosContextFactory.CreateDbContext();
                _videosContext.VideoIds.Add(new VideoId()
                {
                    Id = id,
                    AddDate = _file.CreatedTime,
                    Userid = _userId,
                    Videoname = file.Name[0..^4],
                    ThumbnailLink = thumbnailLink,
                    Videoid = videoId
                });
                _videosContext.SaveChanges();

                // Post video if user has the option on true
                if (_videosContext.User.First(u => u.Userid == _userId).Post == true)
                {
                    _discordService.PostVideo(_appSettings.Backend + "video/" + id).Wait();
                }
            });
        }
    }
}
