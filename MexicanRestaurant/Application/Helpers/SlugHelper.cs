using System.Text.RegularExpressions;

namespace MexicanRestaurant.Application.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            string str = title.ToLowerInvariant().Trim();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-").Trim();
            str = Regex.Replace(str, @"-+", "-");     
            
            return str;
        }
    }
}
