using Mirror2MegaNZ.Logic;
using NUnit.Framework;
using System;

namespace MegaNZTest1.UnitTests
{
    [TestFixture]
    [Category("NodeComparer")]
    public class NameHandlerTests
    {
        [Test]
        public void ExtractDateTimeFromName_shouldReturnTheCorrectValue()
        {
            var name = "filename-2016_1_1_0_0_0.jpeg";

            var result = NameHandler.ExtractDateTimeFromName(name);

            Assert.AreEqual(new DateTime(2016, 1, 1, 0, 0, 0), result);
        }

        [Test]
        public void BuilRemoteFilename_forAFile_shouldBuildTheCorrectFilename()
        {
            var lastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var filename = "filename.jpg";

            var result = NameHandler.BuildRemoteFileName(filename, lastModificationDate);

            Assert.AreEqual("filename-2016_1_1_0_0_0.jpg", result);

        }

        [Test]
        public void BuilRemoteFolderName_forAFolder_shouldBuildTheCorrectFolderName()
        {
            var folderName = "foldername";

            var result = NameHandler.BuildRemoteFolderName(folderName);

            Assert.AreEqual("foldername", result);
        }
    }
}
