using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using NUnit.Framework;
using System;
using Mirror2MegaNZ.Logic;

namespace MegaNZTest1.UnitTests
{
    [TestFixture]
    [Category("NodeComparer")]
    public class NodeComparerTests
    {
        [Test]
        public void AreTheSameFile_withTwoEqualFiles_shouldReturnTrue()
        {
            var localFile = new LocalNode
            {
                Name = "LocalFile.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFile.jpeg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var remoteFile = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "LocalFile-2016_1_1_0_0_0.jpeg",
                    Type = NodeType.File
                }
            };

            var result = NodeComparer.AreTheSameFile(remoteFile, localFile);

            Assert.IsTrue(result);
        }

        [Test]
        public void AreTheSameFile_withTwoFileWithDifferentName_shouldReturnFalse()
        {
            var localFile = new LocalNode
            {
                Name = "LocalFile.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFile.jpeg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Size = 100
            };

            var remoteFile = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "DifferentName-2016_1_1_0_0_0.jpeg",
                    Type = NodeType.File,
                    Size = 100
                }
            };

            var result = NodeComparer.AreTheSameFile(remoteFile, localFile);

            Assert.IsFalse(result);
        }

        [Test]
       
        public void AreTheSameFile_withOneItemThatIsAFolder_shouldThrowException()
        {
            var localFile = new LocalNode
            {
                Name = "LocalFile.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFile.jpeg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var remoteFolder = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "ThisIsAFolder",
                    Type = NodeType.Directory
                }
            };

            Assert.Throws<InvalidOperationException>(() => NodeComparer.AreTheSameFile(remoteFolder, localFile));
        }

        [Test]
        public void AreTheSameFile_withTwoFileWithDifferentSize_shouldReturnFalse()
        {
            var localFile = new LocalNode
            {
                Name = "LocalFile.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFile.jpeg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Size = 100
            };

            var remoteFile = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "LocalFile-2016_1_1_0_0_0.jpeg",
                    Type = NodeType.File,
                    LastModificationDate = localFile.LastModificationDate,
                    Size = 1
                }
            };

            var result = NodeComparer.AreTheSameFile(remoteFile, localFile);

            Assert.IsFalse(result);
        }

        [Test]
        public void AreTheSameFile_withTwoFileWithDifferentLastModificationDateTime_shouldReturnFalse()
        {
            var localFile = new LocalNode
            {
                Name = "LocalFile.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFile.jpeg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Size = 100
            };

            var remoteFile = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "LocalFile-2016_1_1_0_0_1.jpeg",
                    Type = localFile.Type,
                    LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 1),
                    Size = localFile.Size
                }
            };

            var result = NodeComparer.AreTheSameFile(remoteFile, localFile);

            Assert.IsFalse(result);
        }

        [Test]
        public void AreTheSameFolder_withTwoEqualFolders_shouldReturnTrue()
        {
            var localFolder = new LocalNode
            {
                Name = "LocalFolder",
                Type = NodeType.Directory,
                FullPath = @"c:\somedirectory\LocalFolder",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var remoteFolder = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "LocalFolder",
                    Type = NodeType.Directory,
                    LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 1)
                }
            };

            var result = NodeComparer.AreTheSameFolder(remoteFolder, localFolder);

            Assert.IsTrue(result);
        }

        [Test]
        public void AreTheSameFolder_withTwoFoldersWithDifferentName_shouldReturnFalse()
        {
            var localFolder = new LocalNode
            {
                Name = "LocalFolder",
                Type = NodeType.Directory,
                FullPath = @"c:\somedirectory\LocalFolder",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var remoteFolder = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "AnotherLocalFolder",
                    Type = NodeType.Directory,
                    LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 1)
                }
            };

            var result = NodeComparer.AreTheSameFolder(remoteFolder, localFolder);

            Assert.IsFalse(result);
        }
    }
}
