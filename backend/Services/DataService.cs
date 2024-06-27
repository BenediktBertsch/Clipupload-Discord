using Newtonsoft.Json;
using System.Net.Http.Headers;
using Backend.Models;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class DataService
    {
        private readonly HttpClient _client;
        private readonly DiscordSettings _discordSettings;
        private readonly AppSettings _appSettings;
        private readonly string _tokenUrl;
        public DataService(HttpClient client, IOptions<DiscordSettings> discordSettings, IOptions<AppSettings> appSettings)
        {
            _client = client;
            _discordSettings = discordSettings.Value;
            _appSettings = appSettings.Value;
            _tokenUrl = _discordSettings.EndPoint + "/oauth2/token";
        }

        internal async Task<Token> GetAccessToken(string code, string? redirecturi)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, _tokenUrl) { 
                Content = new FormUrlEncodedContent(GenerateParameters(code, redirecturi, false))
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var response = await _client.SendAsync(req).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(content);
        }

        internal async Task<Token> GetAccessRefreshToken(string refresh_token, string? redirecturi)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, _tokenUrl)
            {
                Content = new FormUrlEncodedContent(GenerateParameters(refresh_token, redirecturi, true))
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var response = await _client.SendAsync(req).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(content);
        }

        private List<KeyValuePair<string,string>> GenerateParameters(string code, string? redirecturi, bool refresh)
        {
            if (refresh)
            {
                return new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("client_id", _discordSettings.IdBot),
                    new KeyValuePair<string, string>("client_secret",  _discordSettings.SecretBot),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", code)
                };
            }
            else
            {
                if (redirecturi != null)
                {
                    return new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("client_id", _discordSettings.IdBot),
                        new KeyValuePair<string, string>("client_secret",  _discordSettings.SecretBot),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>( "redirect_uri", redirecturi),
                        new KeyValuePair<string, string>( "scope", Uri.EscapeDataString(_discordSettings.ScopeBot))
                    };
                }
                else
                {
                    var url = _appSettings.Frontend.Last() == '/' ? _appSettings.Frontend : _appSettings.Frontend + '/';
                    return new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("client_id", _discordSettings.IdBot),
                        new KeyValuePair<string, string>("client_secret",  _discordSettings.SecretBot),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", url),
                        new KeyValuePair<string, string>( "scope", Uri.EscapeDataString(_discordSettings.ScopeBot))
                    };
                }
            }
        }
    }
}