using Mirror2MegaNZ.V2.Logic;
using Moq;
using NUnit.Framework;
using SystemInterface.IO;
using FluentAssertions;
using SystemWrapper;
using System;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class LocalFileItemListGeneratorTests
    {
        [Test]
        public void Generate_withABaseWithoutTheEndingSlash_shouldGenerateTheCorrectPathAndName()
        {
            // We want to check that the code add the needed slash at the end of the
            // base path
            // Give we have the following:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> Folder2
            // And having the "\Folder1" as base path (without the ending slash)
            // Then the generated item for the root must have the correct path

            var mockFolder2 = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder2.SetupGet(m => m.FullName).Returns(@"c:\folder1\folder2");
            mockFolder2.SetupGet(m => m.Name).Returns("folder2");
            mockFolder2.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);
            mockFolder2.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);

            var mockFolder1 = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1.SetupGet(m => m.FullName).Returns(@"c:\folder1");
            mockFolder1.SetupGet(m => m.Name).Returns("folder1");
            mockFolder1.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder2.Object });
            mockFolder1.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            
            // Act
            var basePath = @"c:\folder1";
            var generator = new LocalFileItemListGenerator();
            var result = generator.Generate(mockFolder1.Object, basePath);

            // Assert
            result.Count.Should().Be(2);
            result[0].Name.Should().Be(@"\");
            result[0].Path.Should().Be(@"\");
            result[1].Name.Should().Be("folder");
            result[1].Path.Should().Be(@"\folder2");
        }

        [Test]
        public void Generate_withoutABasePath_shouldGenerateTheCorrectListOfItem()
        {
            // Given a tree like this in the file system:
            // root
            //    +--> folder1A
            //    |          +--> folder2A
            //    |                     +--> file2A - LastModified: 2016-1-1 00:00:00
            //    +--> folder1B
            //                +--> file2B - LastModified: 2016-1-2 02:00:00
            //                |
            //                +--> file3B - LastModified: 2016-1-3 03:00:00
            // Having no base path, we should get the following list of items:
            // - \ (root)
            // - \folder1A\
            // - \folder1A\folder2A\
            // - \folder1A\folder2A\file2A.jpeg
            // - \folder1B\
            // - \folder1B\file2B.jpeg
            // - \folder1B\file3B.jpeb
            // with the correct Last Modified property

            var mockFile2A = new Mock<IFileInfo>(MockBehavior.Strict);
            var file2ALastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            mockFile2A.SetupGet(m => m.FullName).Returns(@"\folder1A\folder2A\file2A.jpeg");
            mockFile2A.SetupGet(m => m.Name).Returns("file2A.jpeg");
            mockFile2A.SetupGet(m => m.Length).Returns(1024);
            mockFile2A.SetupGet(m => m.LastWriteTimeUtc).Returns(new DateTimeWrap(file2ALastModifiedDate));

            var mockFolder2A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder2A.SetupGet(m => m.FullName).Returns(@"\folder1A\folder2A");
            mockFolder2A.Setup(m => m.GetFiles()).Returns(new[] { mockFile2A.Object });
            mockFolder2A.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);

            var mockFolder1A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1A.SetupGet(m => m.FullName).Returns(@"\folder1A");
            mockFolder1A.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            mockFolder1A.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder2A.Object });

            var mockFile2B = new Mock<IFileInfo>(MockBehavior.Strict);
            var file2BLastModifiedDate = new DateTime(2016, 1, 2, 0, 0, 0);
            mockFile2B.SetupGet(m => m.FullName).Returns(@"\folder1B\file2B.jpeg");
            mockFile2B.SetupGet(m => m.Name).Returns("file2B.jpeg");
            mockFile2B.SetupGet(m => m.Length).Returns(1024);
            mockFile2B.SetupGet(m => m.LastWriteTimeUtc).Returns(new DateTimeWrap(file2BLastModifiedDate));

            var mockFile3B = new Mock<IFileInfo>(MockBehavior.Strict);
            var file3BLastModifiedDate = new DateTime(2016, 1, 3, 0, 0, 0);
            mockFile3B.SetupGet(m => m.FullName).Returns(@"\folder1B\file3B.jpeg");
            mockFile3B.SetupGet(m => m.Name).Returns("file3B.jpeg");
            mockFile3B.SetupGet(m => m.Length).Returns(1024);
            mockFile3B.SetupGet(m => m.LastWriteTimeUtc).Returns(new DateTimeWrap(file3BLastModifiedDate));

            var mockFolder1B = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1B.SetupGet(m => m.FullName).Returns(@"\folder1B");
            mockFolder1B.Setup(m => m.GetFiles()).Returns(new[] { mockFile2B.Object, mockFile3B.Object });
            mockFolder1B.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);

            var mockRoot = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockRoot.SetupGet(m => m.FullName).Returns(@"\");
            mockRoot.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            mockRoot.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder1A.Object, mockFolder1B.Object });

            // Act
            var basePath = @"\";
            var generator = new LocalFileItemListGenerator();
            var result = generator.Generate(mockRoot.Object, basePath);

            // Assert
            result.Count.Should().Be(7);
            result[0].Path.Should().Be(@"\");
            result[1].Path.Should().Be(@"\folder1A");
            result[2].Path.Should().Be(@"\folder1A\folder2A");
            result[3].Path.Should().Be(@"\folder1A\folder2A\file2A.jpeg");
            result[3].LastModified.Should().Be(file2ALastModifiedDate);
            result[4].Path.Should().Be(@"\folder1B");
            result[5].Path.Should().Be(@"\folder1B\file2B.jpeg");
            result[5].LastModified.Should().Be(file2BLastModifiedDate);
            result[6].Path.Should().Be(@"\folder1B\file3B.jpeg");
            result[6].LastModified.Should().Be(file3BLastModifiedDate);
        }

        [Test]
        public void Generate_withBasePath_shouldGenerateTheCorrectListOfItem()
        {
            // Given a tree like this in the file system:
            // c:\basepath\
            //            |
            //            +--> folder0
            //                       +--> folder1A
            //                       |
            //                       +--> file1A.jpeg - Last modified = 2016-1-1 00:00:00
            // Having c:\basepath\ as base path, we should get the following list of items:
            // - \ (root)
            // - \folder0
            // - \folder0\folder1A\
            // - \folder0\folder1A\file1A.jpeg
            // with the correct Last Modified property
            const string basePath = @"c:\basepath\";
            var file1LastModified = new DateTime(2016, 1, 1, 0, 0, 0);
            var mockFile1A = new Mock<IFileInfo>(MockBehavior.Strict);
            mockFile1A.SetupGet(m => m.FullName).Returns(basePath + @"folder0\file1A.jpeg");
            mockFile1A.SetupGet(m => m.Name).Returns("file1A.jpeg");
            mockFile1A.SetupGet(m => m.Length).Returns(1024);
            mockFile1A.SetupGet(m => m.LastWriteTimeUtc).Returns(new DateTimeWrap(file1LastModified));

            var mockFolder1A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1A.SetupGet(m => m.FullName).Returns(basePath + @"folder0\folder1A");
            mockFolder1A.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            mockFolder1A.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);

            var mockFolder0 = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder0.SetupGet(m => m.FullName).Returns(basePath + @"folder0");
            mockFolder0.Setup(m => m.GetFiles()).Returns(new[] { mockFile1A.Object });
            mockFolder0.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder1A.Object });

            var mockBasePath = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockBasePath.SetupGet(m => m.FullName).Returns(basePath);
            mockBasePath.Setup(m => m.GetFiles()).Returns(new IFileInfo[0] );
            mockBasePath.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder0.Object });

            // Act
            var generator = new LocalFileItemListGenerator();
            var result = generator.Generate(mockBasePath.Object, basePath);

            // Assert
            result.Count.Should().Be(4);
            result[0].Path.Should().Be(@"\");
            result[1].Path.Should().Be(@"\folder0");
            result[2].Path.Should().Be(@"\folder0\file1A.jpeg");
            result[2].LastModified.Should().Be(file1LastModified);
            result[3].Path.Should().Be(@"\folder0\folder1A");
        }
    }
}
