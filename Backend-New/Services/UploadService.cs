using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class UploadService
    {
        private ulong _userId;
        private AppSettings? _appSettings;
        private DiscordService? _discordService;
        private IDbContextFactory<VideosContext>? _videosContextFactory;
        public void UploadStart(ulong userId, DiscordService discordService, AppSettings appSettings, IDbContextFactory<VideosContext> videosContextFactory)
        {
            _discordService = discordService;
            _appSettings = appSettings;
            _videosContextFactory = videosContextFactory;
            _userId = userId;
        }

        private void UploadFinished()
        {
            // Change Permissions, so that everyone can watch the video
            string videoId = "";
            string thumbnailLink = "";
            Console.WriteLine("UPLOAD FINISHED, doing NOTHING!");
            //// Check if the video is processed
            //Task.Run(() =>
            //{
            //    // Get the uploaded file for createdtime attribute
            //    var id = Utils.GenerateId(_videosContextFactory);
            //    var _videosContext = _videosContextFactory.CreateDbContext();
            //    _videosContext.VideoIds.Add(new VideoId()
            //    {
            //        Id = id,
            //        User = _userId,
            //        ThumbnailLink = thumbnailLink,
            //        Videohash = MD5.Create()
            //    });
            //    _videosContext.SaveChanges();

            //    // Post video if user has the option on true
            //    if (_videosContext.User.First(u => u.Userid == _userId).Post == true)
            //    {
            //        _discordService.PostVideo(_appSettings.Backend + "video/" + id).Wait();
            //    }
            //});
        }
    }
}
