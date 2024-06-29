using Backend.Models;
using FFMpegCore.Enums;
using FFMpegCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend
{
    public class Utils
    {
        public static bool CheckProperty(dynamic obj, string property)
        {
            return ((Type)obj.GetType()).GetProperties().Where(p => p.Name.Equals(property)).Any();
        }

        public static string GenerateHash(MD5 md5Alg, Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in md5Alg.ComputeHash(stream))
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static string GenerateId(VideosContext videos)
        {
            string uid;
            do
            {
                uid = Guid.NewGuid().ToString();
            } while (videos.VideoIds.Any(v => v.Id == uid));
            return uid;
        }

        public static bool GenerateThumbnail(string videoFilePath, string thumbnailPath)
        {
            try
            {
                IMediaAnalysis source = FFProbe.Analyse(videoFilePath);
                int streamIndex = source.PrimaryVideoStream?.Index
                    ?? source.VideoStreams.FirstOrDefault()?.Index
                    ?? 0;
                var args = FFMpegArguments
                    .FromFileInput(videoFilePath, false, options => options
                    .Seek(TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3)));
                var opts = new Action<FFMpegArgumentOptions>(options => options.SelectStream((int)streamIndex, 0)
                                                .WithVideoCodec(VideoCodec.LibaomAv1)
                                                .WithFrameOutputCount(1)
                                                .Resize(null));
                args.OutputToFile(thumbnailPath, false, opts).ProcessSynchronously();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        public static bool CreateFile(Stream stream, string filePath)
        {
            try
            {
                FileStream newFile = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                newFile.Write(bytes, 0, bytes.Length);
                stream.Close();
                newFile.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
