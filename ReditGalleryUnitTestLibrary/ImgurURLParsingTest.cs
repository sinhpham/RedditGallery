using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RedditGallery.Models;

namespace ReditGalleryUnitTestLibrary
{
    [TestClass]
    public class ImgurURLParsingTest
    {
        [TestMethod]
        public void MobileURL()
        {
            var gallery = new List<InternetImage>();
            var ii = ImgUrlExtractor.Extract("http://m.imgur.com/2r9QSsy", out gallery);

            Assert.AreEqual(ii.ImageLink, "http://i.imgur.com/2r9QSsy.jpg");
            Assert.AreEqual(ii.ThumbnailLink, "http://i.imgur.com/2r9QSsym.jpg");
        }
    }
}
