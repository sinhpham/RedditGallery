﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.UI.Core;

namespace RedditGallery.Models
{

    public class InternetImage
    {
        public string ImageLink { get; set; }
        public string ThumbnailLink { get; set; }
    }

    public class RedditImage : RedditGallery.ViewModels.NotifyingClass
    {
        InternetImage _displayingImage;
        public InternetImage DisplayingImage
        {
            get { return _displayingImage; }
            set { SetProperty(ref _displayingImage, value); OnPropertyChanged("HasMultipleImages"); }
        }
        public string Title { get; set; }
        public string Permalink { get; set; }
        public string OriginalUrl { get; set; }
        public bool NSFW { get; set; }

        public List<InternetImage> GalleryImages { get; set; }

        public bool HasMultipleImages
        {
            get
            {
                return GalleryImages != null && GalleryImages.Count > 0;
            }
        }
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
                var itemObject = jVal.GetObject()["data"].GetObject();
                var tentativeThumbnail = itemObject["thumbnail"].GetString();
                
                if (!Uri.IsWellFormedUriString(tentativeThumbnail, UriKind.Absolute)) {
                    return null;
                }

                var item = new RedditImage()
                {
                    Title = System.Net.WebUtility.HtmlDecode(itemObject["title"].GetString()),
                    Permalink = "http://reddit.com" + itemObject["permalink"].GetString(),
                    OriginalUrl = itemObject["url"].GetString(),
                    NSFW = itemObject["over_18"].GetBoolean(),
                };

                // Parser task here to determine the correct image and album
                Task.Run(async () =>
                {
                    var extractRet = await ImgUrlExtractor.Extract(itemObject["url"].GetString());
                    var linkedImg = extractRet.Item1;
                    var galleryUrls = extractRet.Item2;

                    if (linkedImg != null && linkedImg.ThumbnailLink == null)
                    {
                        linkedImg.ThumbnailLink = itemObject["thumbnail"].GetString();
                    }

                    if (linkedImg == null && galleryUrls != null && galleryUrls.Count > 0)
                    {
                        linkedImg = galleryUrls[0];
                    }

                    // Modify UI, so need to run this on UI thread.
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        item.DisplayingImage = linkedImg;
                    });

                    item.GalleryImages = galleryUrls;
                });

                return item;
            }).Where(item => item != null).ToList();

            return ret;
        }
    }

    public static class ImgUrlExtractor
    {
        public static async Task<Tuple<InternetImage, List<InternetImage>>> Extract(string inputUrl)
        {
            var ret = new InternetImage() { ImageLink = inputUrl };
            List<InternetImage> galleryUrls = null;

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
                    string albumPage = null;
                    try
                    {
                        albumPage = await hc.GetStringAsync(albumBlogLayoutUrl);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Ex: {0}", e.Message);
                    }
                    if (albumPage != null)
                    {
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(albumPage);
                        if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0 && htmlDoc.DocumentNode != null)
                        {
                            var imgDivs = htmlDoc.DocumentNode.Descendants().Where(n => n.Name == "div" && n.Attributes.FirstOrDefault(a => a.Name == "class" && a.Value == "image") != null);
                            galleryUrls = imgDivs.Select(iDiv =>
                            {
                                var imgNode = iDiv.Descendants().Where(n => n.Name == "img");
                                if (imgNode.Count() != 1)
                                {
                                    Debug.WriteLine("Error in parsing");
                                }

                                var imgLink = string.Format("http:{0}", imgNode.First().GetAttributeValue("data-src", "not found"));
                                return new InternetImage()
                                {
                                    ImageLink = imgLink,
                                    ThumbnailLink = GetThumbnailPathFromUrl(imgLink)
                                };
                            }).ToList();
                        }
                    }
                }
                else if (u.AbsolutePath.StartsWith("/gallery/"))
                {
                    // TODO: parse gallery links.
                    var a = 0;
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
            else if (u.Host == "i.imgur.com")
            {
                // Direct link to an image.
                ret.ThumbnailLink = GetThumbnailPathFromUrl(ret.ImageLink);
            }

            return Tuple.Create(ret, galleryUrls);
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
