using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Middleware
{
    public class Filter
    {
        public required string Path { get; set; }
        public required string Method { get; set; }
    }
    public class AuthenticationMiddleware : IMiddleware
    {
        private readonly VideosContext _videos;
        private readonly DiscordService _discordService;
        public AuthenticationMiddleware(VideosContext videos, DiscordService discordService)
        {
            _videos = videos;
            _discordService = discordService;
        }
        private readonly List<Filter> _middlewareFilter = new() {
            new Filter { Path = "/videos", Method = HttpMethods.Get },
            new Filter { Path = "/upload", Method = HttpMethods.Post },
            new Filter { Path = "/upload/post", Method = HttpMethods.Get },
            new Filter { Path = "/video/", Method = HttpMethods.Delete }, //special case, variable id at the end of the url
            new Filter { Path = "/post", Method = HttpMethods.Patch },
            new Filter { Path = "/post", Method = HttpMethods.Get }
        };

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method == "")
            {
                context.Request.Method = HttpMethods.Get;
            }
            var pathCheck = _middlewareFilter.Any(f => f.Path == context.Request.Path.Value);
            var methodCheck = _middlewareFilter.Where(f => f.Path == context.Request.Path.Value).ToList().Any(m => m.Method == context.Request.Method);
            if (context.Request.Path.Value.StartsWith(_middlewareFilter[3].Path) && _middlewareFilter[3].Method == context.Request.Method || // Special case
                pathCheck && methodCheck)
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader == null)
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.StatusCode = 401;
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("{ \"error\": \"Could not validate the authentication!\"}"));
                    return;
                }
                context.Items["userid"] = await DiscordService.GetId(authHeader);
                if (authHeader != null && authHeader.StartsWith("Bearer") && _discordService.CheckUserGroup((ulong)context.Items["userid"]))
                {
                    if(!_videos.User.AsQueryable().Any(u => u.Id == (ulong)context.Items["userid"]))
                    {
                        _videos.User.Add(new User() { Id = (ulong)context.Items["userid"], Post = true });
                        await _videos.SaveChangesAsync();
                    }
                    await next.Invoke(context);
                }
                else
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.StatusCode = 401;
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("{ \"error\": \"Could not validate the authentication!\"}"));
                    return;
                }
            } else
            {
                await next.Invoke(context);
            }
        }
    }
}
