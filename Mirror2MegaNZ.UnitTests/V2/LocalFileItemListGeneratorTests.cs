using Mirror2MegaNZ.V2.Logic;
using Moq;
using NUnit.Framework;
using SystemInterface.IO;
using FluentAssertions;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class LocalFileItemListGeneratorTests
    {
        [Test]
        public void Generate_withoutABasePath_shouldGenerateTheCorrectListOfItem()
        {
            // Given a tree like this in the file system:
            // root
            //    +--> folder1A
            //    |          +--> folder2A
            //    |                     +--> file2A
            //    +--> folder1B
            //                +--> file2B
            //                |
            //                +--> file3B
            // Having no base path, we should get the following list of items:
            // - \ (root)
            // - \folder1A\
            // - \folder1A\folder2A\
            // - \folder1A\folder2A\file2A.jpeg
            // - \folder1B\
            // - \folder1B\file2B.jpeg
            // - \folder1B\file3B.jpeb

            var mockFile2A = new Mock<IFileInfo>(MockBehavior.Strict);
            mockFile2A.SetupGet(m => m.FullName).Returns(@"\folder1A\folder2A\file2A.jpeg");
            mockFile2A.SetupGet(m => m.Name).Returns("file2A.jpeg");
            mockFile2A.SetupGet(m => m.Length).Returns(1024);

            var mockFolder2A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder2A.SetupGet(m => m.FullName).Returns(@"\folder1A\folder2A\");
            mockFolder2A.Setup(m => m.GetFiles()).Returns(new[] { mockFile2A.Object });
            mockFolder2A.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);

            var mockFolder1A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1A.SetupGet(m => m.FullName).Returns(@"\folder1A\");
            mockFolder1A.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            mockFolder1A.Setup(m => m.GetDirectories()).Returns(new[] { mockFolder2A.Object });

            var mockFile2B = new Mock<IFileInfo>(MockBehavior.Strict);
            mockFile2B.SetupGet(m => m.FullName).Returns(@"\folder1B\file2B.jpeg");
            mockFile2B.SetupGet(m => m.Name).Returns("file2B.jpeg");
            mockFile2B.SetupGet(m => m.Length).Returns(1024);

            var mockFile3B = new Mock<IFileInfo>(MockBehavior.Strict);
            mockFile3B.SetupGet(m => m.FullName).Returns(@"\folder1B\file3B.jpeg");
            mockFile3B.SetupGet(m => m.Name).Returns("file3B.jpeg");
            mockFile3B.SetupGet(m => m.Length).Returns(1024);

            var mockFolder1B = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1B.SetupGet(m => m.FullName).Returns(@"\folder1B\");
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
            result[1].Path.Should().Be(@"\folder1A\");
            result[2].Path.Should().Be(@"\folder1A\folder2A\");
            result[3].Path.Should().Be(@"\folder1A\folder2A\file2A.jpeg");
            result[4].Path.Should().Be(@"\folder1B\");
            result[5].Path.Should().Be(@"\folder1B\file2B.jpeg");
            result[6].Path.Should().Be(@"\folder1B\file3B.jpeg");
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
            //                       +--> file1A.jpeg
            // Having c:\basepath\ as base path, we should get the following list of items:
            // - \ (root)
            // - \folder0
            // - \folder0\folder1A\
            // - \folder0\folder1A\file1A.jpeg
            const string basePath = @"c:\basepath\";

            var mockFile1A = new Mock<IFileInfo>(MockBehavior.Strict);
            mockFile1A.SetupGet(m => m.FullName).Returns(basePath + @"folder0\file1A.jpeg");
            mockFile1A.SetupGet(m => m.Name).Returns("file1A.jpeg");
            mockFile1A.SetupGet(m => m.Length).Returns(1024);

            var mockFolder1A = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder1A.SetupGet(m => m.FullName).Returns(basePath + @"folder0\folder1A\");
            mockFolder1A.Setup(m => m.GetFiles()).Returns(new IFileInfo[0]);
            mockFolder1A.Setup(m => m.GetDirectories()).Returns(new IDirectoryInfo[0]);

            var mockFolder0 = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            mockFolder0.SetupGet(m => m.FullName).Returns(basePath + @"folder0\");
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
            result[1].Path.Should().Be(@"\folder0\");
            result[2].Path.Should().Be(@"\folder0\file1A.jpeg");
            result[3].Path.Should().Be(@"\folder0\folder1A\");
        }
    }
}
