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

        [TestMethod]
        public void DirectImgLink()
        {

            var l = "http://i.imgur.com/403hsL9.jpg";

            var gallery = new List<InternetImage>();
            var ii = ImgUrlExtractor.Extract(l, out gallery);

            Assert.AreEqual(ii.ImageLink, l);
            Assert.AreEqual(ii.ThumbnailLink, "http://i.imgur.com/403hsL9m.jpg");
        }

        [TestMethod]
        public void GalleryImgLink()
        {

            var inputLink = "http://imgur.com/gallery/MpoHLv0";

            var gallery = new List<InternetImage>();
            var ii = ImgUrlExtractor.Extract(inputLink, out gallery);

            Assert.AreEqual(ii.ImageLink, inputLink);
            Assert.AreEqual(ii.ThumbnailLink, null);
        }
    }
}
