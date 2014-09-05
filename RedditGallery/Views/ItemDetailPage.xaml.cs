﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using RedditGallery.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RedditGallery.Models;
using System.Diagnostics;
using RedditGallery.ViewModels;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace RedditGallery.Views
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : Page
    {
        private NavigationHelper navigationHelper;

        public MainVM VM
        {
            get { return App.MainVM; }
        }

        private ObservableDictionary pageDataContext = new ObservableDictionary();
        public ObservableDictionary PageDataContext
        {
            get { return this.pageDataContext; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

            PageDataContext["ImageLoading"] = true;
            PageDataContext["ImageFailed"] = false;
            PageDataContext["ShowBackButton"] = false;

            PageDataContext["MaxHeight"] = Window.Current.Bounds.Height;
            PageDataContext["MaxWidth"] = Window.Current.Bounds.Width;

            if (VM.SelectedItem.NSFW && App.SettingVM.FilterNSFW)
            {
                PageDataContext["ShowNSFWWarning"] = true;

            }
            else
            {
                // Set image source binding in code to prevent flashing of NSFW content.
                PageDataContext["ShowNSFWWarning"] = false;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            object navigationParameter;
            if (e.PageState != null && e.PageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = e.PageState["SelectedItem"];
            }

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void _outerSv_Loaded(object sender, RoutedEventArgs e)
        {
            var outerSv = (ScrollViewer)sender;
            outerSv.ChangeView(null, 60.0f, null);
        }

        bool _isPullRefresh = false;
        private void _outerSv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;


            //if (sv.VerticalOffset == 0.0f)
            //    textBlock1.Opacity = 1;
            //else
            //    textBlock1.Opacity = 0.3f;

            if (sv.VerticalOffset != 0.0f)
                _isPullRefresh = true;

            if (!e.IsIntermediate)
            {
                if (sv.VerticalOffset == 0.0f && _isPullRefresh)
                {
                    Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        navigationHelper.GoBackCommand.Execute(null);
                    });
                }
                _isPullRefresh = false;
                //sv.ChangeView(null, 60.0f, null);
            }
        }

        private void _outerSv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_sv.Width = e.NewSize.Width;
            //_sv.Height = e.NewSize.Height;
            //_outerSv.ChangeView(null, 60.0f, null);
        }

        private void _img_ImageOpened(object sender, RoutedEventArgs e)
        {
            PageDataContext["ImageLoading"] = false;

            var img = (Image)sender;
            var imgGrid = (Grid)img.Parent;
            imgGrid.Width = img.ActualWidth != 0 ? img.ActualWidth : imgGrid.Width;
        }

        private void _img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            PageDataContext["ImageLoading"] = false;
            PageDataContext["ImageFailed"] = true;
        }

        private void pageRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PageDataContext["ShowBackButton"] = true;
        }
    }

    public class RedditImgGalleryVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var rImg = (RedditImage)value;
            if (rImg.GalleryImages != null && rImg.GalleryImages.Count > 0)
            {
                // Has a gallery.
                return rImg.NSFW && App.SettingVM.FilterNSFW ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
