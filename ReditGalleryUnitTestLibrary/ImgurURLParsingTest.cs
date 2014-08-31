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
            var extractRet = ImgUrlExtractor.Extract("http://m.imgur.com/2r9QSsy").Result;

            Assert.AreEqual(extractRet.Item1.ImageLink, "http://i.imgur.com/2r9QSsy.jpg");
            Assert.AreEqual(extractRet.Item1.ThumbnailLink, "http://i.imgur.com/2r9QSsym.jpg");
        }

        [TestMethod]
        public void DirectImgLink()
        {
            var extractRet = ImgUrlExtractor.Extract("http://i.imgur.com/403hsL9.jpg").Result;

            Assert.AreEqual(extractRet.Item1.ImageLink, "http://i.imgur.com/403hsL9.jpg");
            Assert.AreEqual(extractRet.Item1.ThumbnailLink, "http://i.imgur.com/403hsL9m.jpg");
        }

        [TestMethod]
        public void GalleryImgLink()
        {
            var extractRet = ImgUrlExtractor.Extract("http://imgur.com/gallery/MpoHLv0").Result;

            Assert.AreEqual(extractRet.Item1.ImageLink, "http://imgur.com/gallery/MpoHLv0");
            Assert.AreEqual(extractRet.Item1.ThumbnailLink, null);
        }
    }
}
