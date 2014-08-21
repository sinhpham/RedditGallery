using RedditGallery.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace RedditGallery.ViewModels
{
    public class MainVM : BindableBase
    {
        public MainVM()
        {
            _images = new PaginatedCollection<RedditImg>(async (pc, count) =>
            {
                var link = string.Format("http://www.reddit.com/r/{0}/new.json?after={1}", SubReddit, pc.NextPath) ??
                    string.Format("http://www.reddit.com/r/{0}/new.json", SubReddit);

                var hc = new HttpClient();
                var jsonText = await hc.GetStringAsync(link);
                var jsonObject = JsonObject.Parse(jsonText);
                var jArr = jsonObject["data"].GetObject()["children"].GetArray();

                pc.NextPath = jsonObject["data"].GetObject()["after"].GetString();

                var retList = new List<RedditImg>();
                foreach (var itemValue in jArr)
                {
                    var itemObject = itemValue.GetObject()["data"].GetObject();

                    var url = itemObject["url"].GetString();
                    var thumbnail = GetThumbnailPathFromUrl(url) ?? itemObject["thumbnail"].GetString();


                    var item = new RedditImg()
                    {
                        Title = itemObject["title"].GetString(),
                        Thumbnail = thumbnail,
                        ImagePath = url
                    };
                    retList.Add(item);
                }
                return retList;
            });
        }

        private PaginatedCollection<RedditImg> _images;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public PaginatedCollection<RedditImg> Images
        {
            get { return this._images; }
        }

        string _subReddit="pics";
        public string SubReddit
        {
            get { return _subReddit; }
            set
            {
                if (SetProperty(ref _subReddit, value))
                {
                    _images.Clear();
                    Images.NextPath = null;
                }
            }
        }

        private string GetThumbnailPathFromUrl(string inputUrl)
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

    public class PaginatedCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private Func<PaginatedCollection<T>, uint, Task<IEnumerable<T>>> load;
        public bool HasMoreItems { get; protected set; }

        public string NextPath { get; set; }

        public PaginatedCollection(Func<PaginatedCollection<T>, uint, Task<IEnumerable<T>>> load)
        {
            HasMoreItems = true;
            this.load = load;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async c =>
            {
                var data = await load(this, count);

                foreach (var item in data)
                {
                    Add(item);
                }

                HasMoreItems = data.Any();

                return new LoadMoreItemsResult()
                {
                    Count = (uint)data.Count(),
                };
            });
        }
    }
}
