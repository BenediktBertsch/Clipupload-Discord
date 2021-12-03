using Backend.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class GoogleService
    {
        private readonly DriveService _driveService;
        private readonly string _driveId;
        private readonly bool _ready = false;
        private readonly VideosContext _videoContext;
        private readonly DiscordService _discordService;
        private readonly GoogleSettings _googleSettings;
        private readonly AppSettings _appSettings;
        private readonly IDbContextFactory<VideosContext> _videosContextFactory;
        public GoogleService(VideosContext videoContext, DiscordService discordService, IOptions<GoogleSettings> googleSettings, IOptions<AppSettings> appSettings, IDbContextFactory<VideosContext> videosContextFactory)
        {
            _videoContext = videoContext;
            _videosContextFactory = videosContextFactory;
            _discordService = discordService;
            _googleSettings = googleSettings.Value;
            _appSettings = appSettings.Value;
            string path;

            // Get the file here: https://console.cloud.google.com/iam-admin/serviceaccounts
            // Create serviceaccount (needs Google Workspace or something similar)
            // Generate new key and select p12 format, place this file here as /Backend/driveapi.p12
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = Environment.CurrentDirectory + "\\driveapi.p12";
            else
                path = Environment.CurrentDirectory + "/driveapi.p12";

            var serviceAccountEmail = _googleSettings.ServiceMail;
            if (System.IO.File.Exists(path))
            {
                // Create Service account
                var certificate = new X509Certificate2(path, "notasecret", X509KeyStorageFlags.Exportable);
                var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveMetadata }
                }.FromCertificate(certificate));
                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Clips Backend"
                });

                try
                {
                    var teamDrives = _driveService.Teamdrives.List().Execute();
                    if (teamDrives.TeamDrives.Count == 0)
                    {
                        Console.WriteLine("No teamdrive found for this service account. Add the service account to one team drive.");
                        Environment.Exit(1);
                    }
                    else
                    {
                        _driveId = teamDrives.TeamDrives[0].Id;
                        _ready = true;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                Console.WriteLine("Drive key file missing (needs a .p12 File).");
                Environment.Exit(1);
            }
        }


        internal async Task Upload(Stream stream, string name, ulong userId)
        {
            if (_ready)
            {
                // Check if userid has a dedicated team folder
                var folderExists = false;
                Google.Apis.Drive.v3.Data.File userFolder = new();

                // Get all folders
                var foldersRequest = _driveService.Files.List();
                foldersRequest.SupportsTeamDrives = true;
                foldersRequest.SupportsAllDrives = true;
                foldersRequest.IncludeItemsFromAllDrives = true;
                foldersRequest.Corpora = "drive";
                foldersRequest.DriveId = _driveId;
                foldersRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed=false";
                foldersRequest.Fields = "files(name,id)";

                var folders = await foldersRequest.ExecuteAsync();

                // Loop through folders
                foreach (var folder in folders.Files)
                {
                    if(folder.Name == userId.ToString())
                    {
                        folderExists = true;
                        userFolder = folder;
                    }
                }

                // If folder doesnt exist create it
                if (!folderExists)
                {
                    var bodyFolder = new Google.Apis.Drive.v3.Data.File
                    {
                        Name = userId.ToString(),
                        MimeType = "application/vnd.google-apps.folder",
                        Parents = new List<string> { _driveId }
                    };
                    var folderCreateRequest = _driveService.Files.Create(bodyFolder);
                    folderCreateRequest.SupportsAllDrives = true;
                    folderCreateRequest.SupportsTeamDrives = true;
                    userFolder = await folderCreateRequest.ExecuteAsync();
                }

                // Uploading to google into the current users directory
                var bodyUpload = new Google.Apis.Drive.v3.Data.File {
                    Name = name,
                    MimeType = "application/octet-stream",
                    Parents = new List<string> { userFolder.Id }
                };
                var requestUpload = _driveService.Files.Create(bodyUpload, stream, "application/octet-stream");
                requestUpload.SupportsTeamDrives = true;
                requestUpload.SupportsAllDrives = true;
                var Upload = new UploadService();
                Upload.UploadStart(userId, requestUpload, _discordService, _driveService, _appSettings, _videosContextFactory);
            }
        }

        internal void DeleteAsync(VideoId video)
        {
            var req = _driveService.Files.Delete(video.Videoid);
            req.SupportsAllDrives = true;
            req.SupportsTeamDrives = true;
            req.Execute();
            _videoContext.Remove(video);
            _videoContext.SaveChanges();
        }
    }
}
