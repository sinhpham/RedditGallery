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

    public class InternetImage
    {
        public string ImageLink { get; set; }
        public string ThumbnailLink { get; set; }
    }

    public class RedditImage : RedditGallery.ViewModels.BindableBase
    {
        InternetImage _displayingImage;
        public InternetImage DisplayingImage
        {
            get { return _displayingImage; }
            set { SetProperty(ref _displayingImage, value); }
        }
        public string Permalink { get; set; }
        public string OriginalUrl { get; set; }
        public bool NSFW { get; set; }

        public List<InternetImage> GalleryImages { get; set; }
    }

    public static class RedditImageParser
    {
        public static List<RedditImage> ParseFromJson(string jsonText, out string nextPath)
        {
            var ret = new List<RedditImage>();

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
                List<InternetImage> galleryUrls = null;

                var itemObject = jVal.GetObject()["data"].GetObject();

                var linkedImg = ImgUrlExtractor.Extract(itemObject["url"].GetString(), out galleryUrls);
                if (linkedImg != null && linkedImg.ThumbnailLink == null)
                {
                    linkedImg.ThumbnailLink = itemObject["thumbnail"].GetString();
                }
                
                var permalink = "http://reddit.com" + itemObject["permalink"].GetString();

                var item = new RedditImage()
                {
                    DisplayingImage = linkedImg,
                    Permalink = permalink,
                    OriginalUrl = itemObject["url"].GetString(),
                    GalleryImages = galleryUrls,
                    NSFW = itemObject["over_18"].GetBoolean()
                };
                if (item.DisplayingImage == null && item.GalleryImages != null && item.GalleryImages.Count > 1)
                {
                    item.DisplayingImage = item.GalleryImages[0];
                }

                if (item.NSFW && App.SettingVM.FilterNSFW)
                {
                    return null;
                }
                return item;
            }).Where(rimg => rimg != null).ToList();

            return ret;
        }
    }

    public static class ImgUrlExtractor
    {
        public static InternetImage Extract(string inputUrl, out List<InternetImage> galleryUrls)
        {
            var ret = new InternetImage() { ImageLink = inputUrl };
            galleryUrls = null;

            var u = new Uri(inputUrl);
            if (u.Host == "m.imgur.com")
            {
                // Turn mobile page into normal page.
                var desktopU = new UriBuilder(u);
                desktopU.Host = "imgur.com";
                u = desktopU.Uri;
            }

            if (u.Host == "imgur.com")
            {
                if (u.AbsolutePath.StartsWith("/a/"))
                {
                    // Imgur albums.
                    galleryUrls = new List<InternetImage>();
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
                        galleryUrls = imgDivs.Select(iDiv =>
                        {
                            var imgLink = string.Format("http://i.imgur.com/{0}.jpg", iDiv.Id);
                            return new InternetImage()
                            {
                                ImageLink = imgLink,
                                ThumbnailLink = GetThumbnailPathFromUrl(imgLink)
                            };
                        }).ToList();

                        if (galleryUrls.Count == 0)
                        {
                            var a = 0;
                        }
                    }
                }
                else
                {
                    // Imgur single image page.
                    var imgLink = "http://i.imgur.com" + u.AbsolutePath;
                    if (!u.AbsolutePath.Contains('.'))
                    {
                        imgLink += ".jpg";
                    }
                    ret.ImageLink = imgLink;
                    ret.ThumbnailLink = GetThumbnailPathFromUrl(imgLink);
                }
            }

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
    }
}
