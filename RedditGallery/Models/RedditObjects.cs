using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace RedditGallery.Models
{
    public class RedditImg
    {
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string ImagePath { get; set; }
        public string Permalink { get; set; }
        public bool NSFW { get; set; }

        public static List<RedditImg> ParseFromJson(string jsonText, out string nextPath)
        {
            var ret = new List<RedditImg>();

            var jsonObject = JsonObject.Parse(jsonText);
            var jArr = jsonObject["data"].GetObject()["children"].GetArray();

            try
            {
                nextPath = jsonObject["data"].GetObject()["after"].GetString();
            }
            catch (InvalidOperationException)
            {
                nextPath = "null";
            }

            ret = jArr.AsParallel().Select(jVal =>
            {
                var itemObject = jVal.GetObject()["data"].GetObject();

                var url = ProcessUrl(itemObject["url"].GetString());
                var thumbnail = GetThumbnailPathFromUrl(url) ?? itemObject["thumbnail"].GetString();
                var permalink = "http://reddit.com" + itemObject["permalink"].GetString();

                var item = new RedditImg()
                {
                    Title = itemObject["title"].GetString(),
                    Thumbnail = thumbnail,
                    ImagePath = url,
                    Permalink = permalink,
                    NSFW = itemObject["over_18"].GetBoolean()
                };
                if (item.NSFW && App.SettingVM.FilterNSFW)
                {
                    return null;
                }
                return item;
            }).Where(rimg => rimg != null).ToList();

            return ret;
        }

        private static string GetThumbnailPathFromUrl(string inputUrl)
        {
            string ret = null;

            var u = new Uri(inputUrl);
            if (u.Host == "i.imgur.com")
            {
                var strArr = u.AbsolutePath.Split('.');
                if (strArr.Length == 2)
                {
                    ret = "http://" + u.Host + strArr[0] + "m." + strArr[1];
                }
            }

            return ret;
        }

        private static string ProcessUrl(string inputUrl)
        {
            var ret = inputUrl;
            var u = new Uri(inputUrl);
            if (u.Host == "imgur.com")
            {
                if (u.AbsolutePath.StartsWith("/a/"))
                {
                    // TODO: handle album
                }
                else
                {
                    ret = "http://i.imgur.com" + u.AbsolutePath;
                    if (!u.AbsolutePath.Contains('.'))
                    {
                        ret += ".jpg";
                    }
                }
            }

            return ret;
        }
    }
}
