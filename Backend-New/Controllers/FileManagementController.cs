using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    public class FileManagementController:ControllerBase
    {
        private readonly VideosContext _videos;

        public FileManagementController(VideosContext videos)
        {
            _videos = videos;
        }

        // Upload video
        [HttpPost]
        [Route("/upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file != null)
            {
                if (file.Length > 0 && file.ContentType == "video/mp4")
                {
                    var stream = file.OpenReadStream();
                    var name = file.FileName;
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
            var userId = HttpContext.Items["userid"];
            if (userId == null)
            {
                return BadRequest(new
                {
                    error = "User not found in HttpContext."
                });
            }
            var video = await _videos.VideoIds.FirstOrDefaultAsync(v => v.Id == id && (ulong)userId == v.User);
            if(video == null)
            {
                return BadRequest(new { error = "Video with this id not found." });
            }
            return Ok(new { success = true });
        }

        // Delete video
        [HttpGet]
        [Route("/video/{id}")]
        public async Task<IActionResult> VideoGetMetadata(string id)
        {
            var video = await _videos.VideoIds.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null)
            {
                return BadRequest(new { error = "Video with this id not found." });
            }
            var username = await DiscordService.GetUsernameOfId(video.User);
            return Ok(new { success = true, name = video.Name, username });
        }
    }
}
