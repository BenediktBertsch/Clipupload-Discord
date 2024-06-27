using Backend.Models;
using Backend.Services;
using Discord;
using Discord.Rest;
using System.Text;

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

                if (authHeader.Split(" ").Length == 0)
                {
                    await BadValidation(context);
                    return;
                }

                var bearerToken = authHeader.Split(" ")[1];
                if (authHeader == null || bearerToken == null)
                {
                    await BadValidation(context);
                    return;
                }

                var restClient = new DiscordRestClient();
                try
                {
                    await restClient.LoginAsync(TokenType.Bearer, bearerToken);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    await BadValidation(context);
                    return;
                }
                
                context.Items["userid"] = restClient.CurrentUser.Id;

                if (await _discordService.CheckUserInGuild(restClient.CurrentUser.Id))
                {
                    if(!_videos.User.AsQueryable().Any(u => u.Id == restClient.CurrentUser.Id))
                    {
                        _videos.User.Add(new User() { Id = restClient.CurrentUser.Id, Post = true });
                        await _videos.SaveChangesAsync();
                    }
                    await restClient.DisposeAsync();
                    await next.Invoke(context);
                }
                else
                {
                    await restClient.DisposeAsync();
                    await BadValidation(context);
                    return;
                }
            } else
            {
                await next.Invoke(context);
            }
        }

        private async Task BadValidation(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 401;
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("{ \"error\": \"Could not validate the authentication!\"}"));
        }
    }
}
