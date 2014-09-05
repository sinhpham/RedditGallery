using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RedditGallery.Views
{
    public sealed partial class RedditImageUserControl : UserControl
    {
        public RedditImageUserControl()
        {
            this.InitializeComponent();
        }

        private void _outerSv_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void _outerSv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void _outerSv_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void _img_ImageOpened(object sender, RoutedEventArgs e)
        {

        }

        private void _img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
