using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Backend
{
    public class Utils
    {
        // Check if Property exists on a dynamic object
        public static bool CheckProperty(dynamic obj, string property)
        {
            return ((Type)obj.GetType()).GetProperties().Where(p => p.Name.Equals(property)).Any();
        }

        // Generate unique Id
        public static string GenerateId(IDbContextFactory<VideosContext> videosContextFactory)
        {
            var context = videosContextFactory.CreateDbContext();
            string uid;
            do
            {
                uid = Guid.NewGuid().ToString();
            } while (context.VideoIds.Any(v => v.Id == uid));
            return uid;
        }
    }
}
