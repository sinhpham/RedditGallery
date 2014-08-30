using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public string OriginalUrl { get; set; }
        public bool NSFW { get; set; }

        public List<string> GalleryImages { get; set; }

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
                List<string> galleryUrls = null;

                var itemObject = jVal.GetObject()["data"].GetObject();

                var url = ProcessUrl(itemObject["url"].GetString(), out galleryUrls);
                var thumbnail = GetThumbnailPathFromUrls(url, galleryUrls) ?? itemObject["thumbnail"].GetString();
                var permalink = "http://reddit.com" + itemObject["permalink"].GetString();

                var item = new RedditImg()
                {
                    Title = itemObject["title"].GetString(),
                    Thumbnail = thumbnail,
                    ImagePath = url,
                    Permalink = permalink,
                    OriginalUrl = itemObject["url"].GetString(),
                    GalleryImages = galleryUrls,
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

        private static string GetThumbnailPathFromUrls(string inputUrl, List<string> galleryUrls)
        {
            var firtImgUrl = inputUrl ?? (galleryUrls.Count > 0 ? galleryUrls[0] : "http://localhost");

            string ret = null;

            var u = new Uri(firtImgUrl);
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

        private static string ProcessUrl(string inputUrl, out List<string> galleryUrls)
        {
            var ret = inputUrl;
            galleryUrls = null;

            var u = new Uri(inputUrl);
            if (u.Host == "imgur.com")
            {
                if (u.AbsolutePath.StartsWith("/a/"))
                {
                    galleryUrls = new List<string>();
                    ret = null;
                    var apArr = u.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    var albumBlogLayoutUrl = "http://" + u.Host + "/" + apArr[0] + "/" + apArr[1] + "/layout/blog";

                    var hc = new HttpClient();
                    var albumPage = hc.GetStringAsync(albumBlogLayoutUrl).Result;

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(albumPage);
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0 && htmlDoc.DocumentNode != null)
                    {
                        var imgDivs = htmlDoc.DocumentNode.Descendants().Where(n => n.Name == "div" && n.Attributes.FirstOrDefault(a => a.Name == "class" && a.Value == "image") != null);
                        galleryUrls = imgDivs.Select(iDiv => string.Format("http://i.imgur.com/{0}.jpg", iDiv.Id)).ToList();

                        if (galleryUrls.Count == 0)
                        {
                            var a = 0;
                        }
                    }
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
