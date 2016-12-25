using FluentAssertions;
using Mirror2MegaNZ.DomainModel;
using Moq;
using NUnit.Framework;
using System;
using SystemInterface.IO;
using SystemWrapper;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    public class FileItemTests
    {
        [Test]
        public void Constructor_usingFileInfo_shouldBuildTheCorrectPath()
        {
            var lastModifiedDate = new DateTimeWrap(new DateTime(2016, 1, 1, 0, 0, 0));
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(m => m.FullName).Returns(@"c:\folder1\folder2\testfile.jpeg");
            mockFileInfo.SetupGet(m => m.Length).Returns(1024);
            mockFileInfo.SetupGet(m => m.LastWriteTimeUtc).Returns(lastModifiedDate);
            var baseFolder = @"c:\folder1\";

            var item = new FileItem(mockFileInfo.Object, baseFolder);

            item.Path.Should().Be(@"\folder2\testfile.jpeg");
            item.Name.Should().Be("testfile.jpeg");
            item.LastModified.Should().HaveValue();
            item.LastModified.Value.Year.Should().Be(lastModifiedDate.Year);
            item.LastModified.Value.Month.Should().Be(lastModifiedDate.Month);
            item.LastModified.Value.Day.Should().Be(lastModifiedDate.Day);
            item.LastModified.Value.Hour.Should().Be(lastModifiedDate.Hour);
            item.LastModified.Value.Minute.Should().Be(lastModifiedDate.Minute);
            item.LastModified.Value.Second.Should().Be(lastModifiedDate.Second);

            mockFileInfo.VerifyAll();
        }

        [Test]
        public void Constructor_usingDirectoryInfo_shouldBuildTheCorrectPath()
        {
            var mockDirectoryInfo = new Mock<IDirectoryInfo>();
            mockDirectoryInfo.SetupGet(m => m.FullName).Returns(@"c:\folder1\folder2\folder3");
            var baseFolder = @"c:\folder1\";

            var item = new FileItem(mockDirectoryInfo.Object, baseFolder);

            item.Path.Should().Be(@"\folder2\folder3\");    // We want to have the backslash at the end of the path for the directory item
            item.Name.Should().Be("folder3");
        }
    }
}
