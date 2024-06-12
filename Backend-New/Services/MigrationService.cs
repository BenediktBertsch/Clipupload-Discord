using Backend.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class MigrationService : IHostedService
    {
        private readonly MigrationSettings _migrationSettings;
        private readonly IDbContextFactory<VideosContext> _videosContextFactory;
        private readonly IDbContextFactory<VideosMigrationContext> _videosMigrationContextFactory;
        public MigrationService(IOptions<MigrationSettings> migrationSettings, IDbContextFactory<VideosContext> videosContextFactory, IDbContextFactory<VideosMigrationContext> videosMigrationContextFactory)
        {
            _migrationSettings = migrationSettings.Value;
            _videosContextFactory = videosContextFactory;
            _videosMigrationContextFactory = videosMigrationContextFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var videosMigration = _videosMigrationContextFactory.CreateDbContext();
            var videos = _videosContextFactory.CreateDbContext();
            var dir = new DirectoryInfo("clips");
            var dirs = dir.GetDirectories();
            var MD5Alg = CryptoConfig.CreateFromName("MD5") as MD5;
            //foreach (var one_dir in dirs)
            //{
            //    await Console.Out.WriteLineAsync(one_dir.Name);
            //    var files = one_dir.GetFiles();
            //    foreach (var file in files)
            //    {
            //        var name = file.Name;
                    //if (name.Contains(".png"))
                    //{
                    //    name = file.FullName.Remove(file.FullName.Length - 4, 4);
                    //    name += ".avif";
                    //    var source = FFProbe.Analyse(file.FullName);
                    //    var streamIndex = source.PrimaryVideoStream?.Index
                    //                    ?? source.VideoStreams.FirstOrDefault()?.Index
                    //                    ?? 0;
                    //    var args = FFMpegArguments
                    //                .FromFileInput(file.FullName, false, options => options
                    //                .Seek(TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3)));
                    //    var opts = new Action<FFMpegArgumentOptions>(options => options.SelectStream((int)streamIndex, 0)
                    //                                                .WithVideoCodec(VideoCodec.LibaomAv1)
                    //                                                .WithFrameOutputCount(1)
                    //                                                .Resize(null));
                    //    args.OutputToFile(name, false, opts).ProcessSynchronously();
                    //    await Console.Out.WriteLineAsync(name);
                    //}
                    //var filetype = ".mp4";
                    //if (name.Contains(".avif"))
                    //{
                    //    filetype = ".avif";
                    //    name = name.Remove(name.Length - 5, 5);
                    //}
                    //else if (name.Contains(".png"))
                    //{
                    //    filetype = ".png";
                    //    name = name.Remove(name.Length - 4, 4);
                    //} else if(name.Contains(".mp4"))
                    //{
                    //    name = name.Remove(name.Length - 4, 4);
                    //}
                    //var search_id = videosMigration.VideoIds.Where(i => i.Id == name && i.Userid == ulong.Parse(one_dir.Name)).ToList();
                    //if (search_id.Count > 0)
                    //{
                    //    await Console.Out.WriteLineAsync("File already renamed, skipping...");
                    //    continue;
                    //}
                    //var matches = videosMigration.VideoIds.Where(i => i.Videoname == name && i.Userid == ulong.Parse(one_dir.Name)).ToList();
                    //    if (matches.Count == 0)
                    //    {
                    //        matches = videosMigration.VideoIds.Where(i => i.Videoname == name + ".mp4" && i.Userid == ulong.Parse(one_dir.Name)).ToList();
                    //    }
                    //for (var i = 0; i < matches.Count; i++)
                    //{
                    //    var match = matches[i];
                    //    await Console.Out.WriteLineAsync($"Renaming {file.FullName} to {one_dir.FullName + "\\" + match.Id + filetype}");
                    //    try
                    //    {
                    //        File.Move(file.FullName, one_dir.FullName + "\\" + match.Id + filetype);
                    //    }
                    //    catch { }
                        
                        //var name_of_file = matches[i].Videoname;
                        //if (name_of_file.Contains(".DVR") && i > 0)
                        //{
                        //    name_of_file = name_of_file.Remove(name_of_file.Length - 4, 4) + $"({i})" + ".DVR";
                        //}
                        //else if (i > 0)
                        //{
                        //    name_of_file += $"({i})";
                        //}

                        //await Console.Out.WriteLineAsync("Adding this file as entry to db: " + name_of_file);
                        //if (name_of_file.Contains(".mp4"))
                        //{
                        //    name_of_file = name_of_file.Remove(name_of_file.Length - 4, 4);
                        //}
                        //var current_file = new FileInfo($"clips\\{one_dir.Name}\\{name_of_file}.mp4");
                        //if (!current_file.Exists)
                        //{
                        //    current_file = new FileInfo($"clips\\{one_dir.Name}\\{name_of_file}");
                        //}

                        //// Creating hash
                        //var hash = MD5Alg.ComputeHash(current_file.OpenRead());
                        //StringBuilder sb = new StringBuilder();
                        //foreach (byte b in hash)
                        //    sb.Append(b.ToString("X2"));
                        //await Console.Out.WriteLineAsync(sb.ToString());

                        //// Creating thumbnail
                        //FFMpeg.Snapshot(current_file.FullName, current_file.FullName + "-thumbnail");
                        //videos.Add(new Video { Date = (DateTime)matches[i].AddDate, Id = matches[i].Id, Name = name_of_file, User = matches[i].Userid, Hash = sb.ToString() });
                    //}
                //}
            //}
            //await videos.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
