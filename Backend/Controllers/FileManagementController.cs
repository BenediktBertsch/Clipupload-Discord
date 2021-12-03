using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    public class FileManagementController:ControllerBase
    {
        private readonly VideosContext _videos;
        private readonly GoogleService _googleService;
        private readonly GoogleSettings _googleSettings;

        public FileManagementController(VideosContext videos, GoogleService googleService, IOptions<GoogleSettings> googleSettings)
        {
            _videos = videos;
            _googleService = googleService;
            _googleSettings = googleSettings.Value;
        }

        // Upload video
        [HttpPost]
        [Route("/upload")]
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
        {
            if (file != null)
            {
                if (file.Length > 0 && file.ContentType == "video/mp4")
                {
                    var stream = file.OpenReadStream();
                    var name = file.FileName;
                    await _googleService.Upload(stream, name, (ulong)HttpContext.Items["userid"]);
                }
                return Ok(new { success = true });
            } else
            {
                return BadRequest(new { success = false, error = "No file attached." });
            }
            
        }

        // Delete video
        [HttpDelete]
        [Route("/video/{id}")]
        public async Task<IActionResult> VideoDelete(string id)
        {
            var video = await _videos.VideoIds.FirstOrDefaultAsync(v => v.Id == id && (ulong)HttpContext.Items["userid"] == v.Userid);
            if(video == null)
            {
                return BadRequest(new { error = "Video with this id not found." });
            }
            _googleService.DeleteAsync(video);
            return Ok(new { success = true });
        }

        // Returns page with Video + Meta Tags for embedding
        [HttpGet]
        [Route("/video/{id}")]
        public async Task<IActionResult> Video(string id)
        {
            var video = await _videos.VideoIds.FirstOrDefaultAsync(i => i.Id == id);
            if (video != null)
            {
                string username = DiscordService.GetUsernameOfId(video.Userid);
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<head>" +
                                "<link rel = \"stylesheet\" href = \"https://cdn.plyr.io/3.6.3/plyr.css\" /> " +
                                "<link href = \"https://fonts.googleapis.com/css2?family=Roboto&amp;display=swap\" rel = \"stylesheet\" />" +
                                "<script src=\"https://cdn.plyr.io/3.6.3/plyr.js\" type=\"text/javascript\"></script> " +
                                "<script> " +
                                    "document.addEventListener(" +
                                    "'DOMContentLoaded', () => {" +
                                    "const player = new Plyr('#player'); " +
                                    "window.player = player; }); " +
                                "</script>" +
                                "<meta property = \"og:type\" content = \"video.other\" />" +
                                "<meta property = \"og:url\" content = \"Gepostet von: " + username + "\" />" +
                                "<meta property = \"og:video:width\" content = \"640\" />" +
                                "<meta property = \"og:video:height\" content = \"426\" />" +
                                "<meta property = \"og:video:type\" content = \"application /x-shockwave-flash\" />" +
                                "<meta property = \"og:video:url\" content = \"https://www.googleapis.com/drive/v3/files/" + video.Videoid + "?key=" + _googleSettings.ApiKey + "&alt=media\" />" +
                                "<meta property = \"og:video:secure_url\" content = \"https://www.googleapis.com/drive/v3/files/" + video.Videoid + "?key=" + _googleSettings.ApiKey + "&alt=media\" />" +
                                "<meta property = \"og:title\" content = \"" + video.Videoname + "\" />" +
                                "<meta property = \"og:image\" content = \"https://lh3.googleusercontent.com/d/" + video.Videoid + "=w1000?authuser=0\" />" +
                                "<meta property = \"og:site_name\" content = \"Gepostet von: " + username + "\" />" +
                            "</head>" +
                            "<body style = \"display: flex; justify-content: center; align-items: center; margin: unset; background: black;\" >" +
                                "<div class=\"plyr__video -wrapper\">" +
                                    "<video " +
                                        "id=\"player\"" +
                                        "src =\"https://www.googleapis.com/drive/v3/files/" + video.Videoid + "?key=" + _googleSettings.ApiKey + "&alt=media\" />" +
                                        "Your browser does not support video" +
                                    "</video>" +
                                    "<div class=\"plyr__poster\" hidden =\"\" />" +
                                "</div>" +
                                "<div class=\"plyr__captions\" />" +
                                    "<button type = \"button\" class=\"plyr__control plyr__control--overlaid\" data -plyr=\"play\" aria -label=\"Play\" >" +
                                        "<svg aria-hidden=\"true\" focusable =\"false\">" +
                                            "<use xlink:href=\"#plyr-play\"></use>" +
                                        "</svg>" +
                                        "<span class=\"plyr__sr -only\"> Play</span>" +
                                    "</button>" +
                                "</div>" +
                            "</body>"
                };
            }
            else
            {
                return BadRequest(new { error = "No video for this ID found." });
            }
        }
    }
}
