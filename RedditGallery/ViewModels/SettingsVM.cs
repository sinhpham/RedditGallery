using RedditGallery.Common;
using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedditGallery.ViewModels
{
    public class SettingsVM : NotifyingClass
    {
        public SettingsVM()
        {

        }

        T GetSettingValueOrDefault<T>(T defaultVal, [CallerMemberName] string key = null)
        {
            return AppSettings.GetValueOrDefault(key, defaultVal);
        }

        bool AddOrUpdateSettingValue(object val, [CallerMemberName] string key = null)
        {
            return AppSettings.AddOrUpdateValue(key, val);
        }

        ISettings AppSettings { get { return CrossSettings.Current; } }

        public bool FilterNSFW
        {
            get { return GetSettingValueOrDefault(true); }
            set
            {
                if (AddOrUpdateSettingValue(value))
                {
                    AppSettings.Save();
                }
                OnPropertyChanged(() => FilterNSFW);
            }
        }

        public bool OpenMenuOnStart
        {
            get { return GetSettingValueOrDefault(true); }
            set
            {
                if (AddOrUpdateSettingValue(value))
                {
                    AppSettings.Save();
                }
                OnPropertyChanged(() => OpenMenuOnStart);
            }
        }

        public string SubList
        {
            get
            {
                var defaultList = new List<string> { "pics", "carporn", "earthporn" };
                var json = Utils.Serialize(defaultList);
                return GetSettingValueOrDefault(json);
            }
            set
            {
                if (AddOrUpdateSettingValue(value))
                {
                    AppSettings.Save();
                }
                OnPropertyChanged(() => SubList);
            }
        }
    }
}
