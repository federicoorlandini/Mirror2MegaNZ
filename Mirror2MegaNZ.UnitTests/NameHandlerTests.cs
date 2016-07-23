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
        public void ExtractFileNameAndDateTimeFromRemoteFilename_shouldReturnTheCorrectName()
        {
            const string originalFilenameWithDateTime = "filename_[[2016-1-1-0-0-0]].jpeg";
            const string originalFilenameWithoutDateTime = "filename.jpeg";

            string extractedFilename;
            DateTime extractedDatetime;
            NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(originalFilenameWithDateTime, out extractedFilename, out extractedDatetime);

            Assert.AreEqual(originalFilenameWithoutDateTime, extractedFilename);
        }

        [Test]
        public void ExtractFileNameAndDateTimeFromName_shouldReturnTheCorrectDateTime()
        {
            const string oringalFilename = "filename_[[2016-1-1-0-0-0]].jpeg";

            string extractedFilename;
            DateTime extractedDatetime;
            NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(oringalFilename, out extractedFilename, out extractedDatetime);

            Assert.AreEqual(new DateTime(2016, 1, 1, 0, 0, 0), extractedDatetime);
        }

        [Test]
        public void BuilRemoteFilename_forAFile_shouldBuildTheCorrectFilename()
        {
            var lastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var filename = "filename.jpg";

            var result = NameHandler.BuildRemoteFileName(filename, lastModificationDate);

            Assert.AreEqual("filename_[[2016-1-1-0-0-0]].jpg", result);

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
