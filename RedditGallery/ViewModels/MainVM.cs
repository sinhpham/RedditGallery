using RedditGallery.Common;
using RedditGallery.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace RedditGallery.ViewModels
{
    public class MainVM : NotifyingClass
    {
        public MainVM()
        {
            _images = new PaginatedCollection<RedditImage>(async (pc, count) =>
            {
                var retList = new List<RedditImage>();
                if (string.Equals("null", pc.NextPath, StringComparison.Ordinal))
                {
                    return retList;
                }

                var link = pc.NextPath != null ?
                    string.Format("http://www.reddit.com/r/{0}/new.json?after={1}&limit={2}", SubReddit, pc.NextPath, count) :
                    string.Format("http://www.reddit.com/r/{0}/new.json?limit={1}", SubReddit, count);

                var hc = new HttpClient();
                var jsonText = await hc.GetStringAsync(link);

                string newNextPath;
                retList = RedditImageParser.ParseFromJson(jsonText, out newNextPath);
                pc.NextPath = newNextPath;

                return retList;
            });

            var subList = Utils.Deserialize<List<string>>(App.SettingVM.SubList);

            foreach (var sub in subList)
            {
                SubReddits.Add(sub);
            }
            SubReddits.CollectionChanged += (s, arg) =>
            {
                App.SettingVM.SubList = Utils.Serialize(SubReddits.ToList());
            };

            if (SubReddits.Count > 0)
            {
                SubReddit = SubReddits[0];
            }
        }

        private PaginatedCollection<RedditImage> _images;
        public PaginatedCollection<RedditImage> Images
        {
            get { return this._images; }
        }

        RedditImage _selectedImg;
        public RedditImage SelectedItem
        {
            get { return _selectedImg; }
            set { SetProperty(ref _selectedImg, value); }
        }

        RelayCommand _copyLinkCmd;
        public RelayCommand CopyLinkCmd
        {
            get
            {
                if (_copyLinkCmd == null)
                {
                    _copyLinkCmd = new RelayCommand(() =>
                    {
                        var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                        dp.SetText(SelectedItem.OriginalUrl);
                        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                    });
                }
                return _copyLinkCmd;
            }
        }

        RelayCommand _copyRedditLinkCmd;
        public RelayCommand CopyRedditLinkCmd
        {
            get
            {
                if (_copyRedditLinkCmd == null)
                {
                    _copyRedditLinkCmd = new RelayCommand(() =>
                    {
                        var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                        dp.SetText(SelectedItem.Permalink);
                        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                    });
                }
                return _copyRedditLinkCmd;
            }
        }

        RelayCommand _openLinkCmd;
        public RelayCommand OpenLinkCmd
        {
            get
            {
                if (_openLinkCmd == null)
                {
                    _openLinkCmd = new RelayCommand(() =>
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(SelectedItem.OriginalUrl));
                    });
                }
                return _openLinkCmd;
            }
        }

        RelayCommand _openRedditLinkCmd;
        public RelayCommand OpenRedditLinkCmd
        {
            get
            {
                if (_openRedditLinkCmd == null)
                {
                    _openRedditLinkCmd = new RelayCommand(() =>
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(SelectedItem.Permalink));
                    });
                }
                return _openRedditLinkCmd;
            }
        }

        RelayCommand _refreshCmd;
        public RelayCommand RefreshCmd
        {
            get
            {
                if (_refreshCmd == null)
                {
                    _refreshCmd = new RelayCommand(() =>
                    {
                        _images.Clear();
                        Images.NextPath = null;
                        Images.HasMoreItems = true;
                    });
                }
                return _refreshCmd;
            }
        }


        public bool IsInFav
        {
            get { return SubReddits.Contains(SubReddit); }
            set
            {
                if (value)
                {
                    // Add to fav.
                    if (!SubReddits.Contains(SubReddit))
                    {
                        SubReddits.Add(SubReddit);
                    }
                }
                else
                {
                    // Remove from fav.
                    SubReddits.Remove(SubReddit);
                }
                OnPropertyChanged(() => IsInFav);
            }
        }

        string _subReddit;
        public string SubReddit
        {
            get { return _subReddit; }
            set
            {
                if (SetProperty(ref _subReddit, value))
                {
                    RefreshCmd.Execute(null);
                    OnPropertyChanged(() => IsInFav);
                }
            }
        }

        ObservableCollection<string> _subReddits = new ObservableCollection<string>();
        public ObservableCollection<string> SubReddits
        {
            get { return _subReddits; }
        }


        public event EventHandler NeedToGoBack;
        public void OnNeedToGoBack()
        {
            var eh = NeedToGoBack;
            if (eh != null)
            {
                eh(this, EventArgs.Empty);
            }
        }
    }

    public class PaginatedCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private Func<PaginatedCollection<T>, uint, Task<IEnumerable<T>>> load;
        public bool HasMoreItems { get; set; }

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
