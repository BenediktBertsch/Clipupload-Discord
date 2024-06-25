using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Backend.Controllers
{
    public class FileManagementController:ControllerBase
    {
        private readonly VideosContext _videos;
        private readonly MD5 _md5alg;
        private readonly FilesSettings _filesSettings;

        public FileManagementController(VideosContext videos, IOptions<FilesSettings> filesSettings)
        {
            _videos = videos;
            _filesSettings = filesSettings.Value;
            _md5alg = (MD5)CryptoConfig.CreateFromName("MD5");
        }

        [HttpPost]
        [Route("/upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            var userId = HttpContext.Items["userid"];
            if (userId == null)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "User not found in HttpContext."
                });
            }

            if (file != null && file.ContentType == "video/mp4" && file.Name != "")
            {
                var name = file.Name;
                if (file.FileName.Contains(".mp4"))
                {
                    name = file.FileName.Remove(file.FileName.Length - 4, 4);
                }

                string hash = Utils.GenerateHash(_md5alg, file.OpenReadStream());
                string id = Utils.GenerateId(_videos);
                string videoPath = _filesSettings.Path + "\\" + userId.ToString() + "\\" + id + ".mp4";
                string thumbnailPath = _filesSettings.Path + "\\" + userId.ToString() + "\\" + id + ".avif";

                var createFileSuccess = Utils.CreateFile(file.OpenReadStream(), videoPath);
                if (!createFileSuccess)
                {
                    return Problem(
                    title: "Server error",
                    detail: "Could not create video file!"
                    );
                }
                var createThumbnail = Utils.GenerateThumbnail(videoPath, thumbnailPath);
                if (!createThumbnail)
                {
                    System.IO.File.Delete(videoPath);
                    return Problem(
                    title: "Server error",
                    detail: "Could not create thumb file!"
                    );
                }

                var newVideo = new Video { Date = new DateTime(), Id = id, Name = name, User = (ulong)userId, Hash = hash };
                _videos.Add(newVideo);
                if(await _videos.SaveChangesAsync() == 1)
                {
                    return Ok(new { success = true, video = newVideo });
                }

                System.IO.File.Delete(videoPath);
                System.IO.File.Delete(thumbnailPath);
                return Problem(
                    title: "Server error",
                    detail: "Could not save entry to DB!"
                );
            }
            else
            {
                return BadRequest(new { success = false, error = "No file attached." });
            }
        }

        [HttpDelete]
        [Route("/video/{id}")]
        public async Task<IActionResult> VideoDelete(string id)
        {
            var userId = HttpContext.Items["userid"];
            if (userId == null)
            {
                return BadRequest(new
                {
                    error = "User not found in HttpContext."
                });
            }
            var video = await _videos.VideoIds.FirstOrDefaultAsync(v => v.Id == id && (ulong)userId == v.User);
            if (video == null)
            {
                return BadRequest(new { error = "Video with this id not found." });
            }
            string videoPath = _filesSettings.Path + "\\" + userId.ToString() + "\\" + id + ".mp4";
            string thumbnailPath = _filesSettings.Path + "\\" + userId.ToString() + "\\" + id + ".avif";
            System.IO.File.Delete(videoPath);
            System.IO.File.Delete(thumbnailPath);

            _videos.Remove(video);
            _videos.SaveChanges();
            return Ok(new { success = true });
        }

        [HttpGet]
        [Route("/video/{id}")]
        public async Task<IActionResult> VideoGetMetadata(string id)
        {
            var video = await _videos.VideoIds.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null)
            {
                return BadRequest(new { error = "Video with this id not found." });
            }
            var username = await DiscordService.GetUsernameById(video.User);
            return Ok(new { success = true, name = video.Name, username });
        }
    }
}
