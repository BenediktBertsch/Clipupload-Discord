using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class UserManagementController:ControllerBase
    {
#nullable enable
        private readonly DataService _dataService;
        private readonly VideosContext _videos;
        public UserManagementController(DataService dataService, VideosContext videos)
        {
            _dataService = dataService;
            _videos = videos;
        }

        // Using Discords API to authenticate User
        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> LoginAsync([FromQuery]string code, string? redirecturi)
        {
            if (code == null)
            {
                return BadRequest(new
                {
                    error = "Invalid parameters."
                });
            } else
            {
                Token req = await _dataService.GetAccessToken(code, redirecturi);
                if(req.AccessToken != null)
                {
                    return Ok(new
                    {
                        access_token = req.AccessToken,
                        expires_in = req.ExpiresIn,
                        refresh_token = req.RefreshToken
                    });
                } else
                {
                    return BadRequest(new
                    {
                        error = "Invalid Code."
                    });
                }
            }
        }

        // Refresh authentication token if expired
        [HttpPost]
        [Route("/refresh")]
        public async Task<IActionResult> Refresh([FromQuery] string refresh_token, [FromQuery]string? redirecturi)
        {
            if (refresh_token == null)
            {
                return BadRequest(new
                {
                    error = "Invalid parameters."
                });
            }
            else
            {
                Token req = await _dataService.GetAccessRefreshToken(refresh_token, redirecturi);
                if (req.AccessToken != null)
                {
                    return Ok(new
                    {
                        access_token = req.AccessToken,
                        expires_in = req.ExpiresIn,
                        refresh_token = req.RefreshToken
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        error = "Invalid Code."
                    });
                }
            }
        }

        // Set if you want to post your uploaded clips or not
        [HttpPatch]
        [Route("/post")]
        public async Task<IActionResult> PatchPost([FromQuery]bool post)
        {
            var userId = HttpContext.Items["userid"];
            if (userId == null)
            {
                return BadRequest(new
                {
                    error = "User not found."
                });
            }    
            var user = _videos.User.AsQueryable().FirstOrDefault(u => u.Id == (ulong)userId);
            if (user != null)
            {
                user.Post = post;
                _videos.User.Update(user);
                await _videos.SaveChangesAsync();
                return Ok(new
                {
                    post = user.Post
                });
            } else
            {
                return BadRequest(new { 
                    error = "User not found."
                });
            }
        }

        // Get Post User
        [HttpGet]
        [Route("/post")]
        public IActionResult GetPost()
        {
            var user = _videos.User.AsQueryable().FirstOrDefault(u => u.Id == (ulong)HttpContext.Items["userid"]);
            if(user == null)
            {
                return BadRequest(new
                {
                    error = "User not found."
                });
            }
            return Ok(new
            {
                post = user.Post
            });
        }

        // Get your own videos
        [HttpGet]
        [Route("/videos")]
        public IActionResult Videos([FromQuery]int? count, [FromQuery]int? offset)
        {
            var strUserId = (string)HttpContext.Items["userid"];
            if (strUserId == null)
            {
                return BadRequest(new
                {
                    error = "User not found in HttpContext."
                });
            }
            var userId = UInt64.Parse(strUserId);
            var allVideos = _videos.VideoIds.AsQueryable().Where(v => v.User == (ulong)userId).OrderByDescending(v => v.Date).ToList();

            if (count != null && offset != null)
            {
                return Ok(new
                {
                    success = true,
                    videos = allVideos.Skip((int)offset).Take((int)count),
                    userId = strUserId,
                    videoCount = count,
                    max = allVideos.Count
                });
            } else
            {
                return Ok(new
                {
                    success = true,
                    videos = allVideos,
                    userId = strUserId,
                });
            }
        }
    }
}
