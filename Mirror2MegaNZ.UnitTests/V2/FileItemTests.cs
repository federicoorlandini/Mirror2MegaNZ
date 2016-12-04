using CG.Web.MegaApiClient;
using FluentAssertions;
using Mirror2MegaNZ.V2.DomainModel;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using SystemInterface.IO;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class FileItemTests
    {
        [Test]
        public void Constructor_usingFileInfo_shouldBuildTheCorrectPath()
        {
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(m => m.Name).Returns("testfile.jpeg");
            mockFileInfo.SetupGet(m => m.FullName).Returns(@"c:\folder1\folder2\testfile.jpeg");
            mockFileInfo.SetupGet(m => m.Length).Returns(1024);
            var baseFolder = @"c:\folder1\";

            var item = new FileItem(mockFileInfo.Object, baseFolder);

            item.Path.Should().Be(@"\folder2\testfile.jpeg");
            mockFileInfo.VerifyAll();
        }

        [Test]
        public void Constructor_usingDirectoryInfo_shouldBuildTheCorrectPath()
        {
            var mockDirectoryInfo = new Mock<IDirectoryInfo>();
            mockDirectoryInfo.SetupGet(m => m.FullName).Returns(@"c:\folder1\folder2\folder3\");
            var baseFolder = @"c:\folder1\";

            var item = new FileItem(mockDirectoryInfo.Object, baseFolder);

            item.Path.Should().Be(@"\folder2\folder3\");
        }
    }
}
