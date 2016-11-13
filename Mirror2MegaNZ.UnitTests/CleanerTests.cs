using NUnit.Framework;
using CG.Web.MegaApiClient;
using Moq;
using Mirror2MegaNZ.DomainModel;
using FluentAssertions;
using NLog;
using System;
using Mirror2MegaNZ.Logic;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    [Category("Cleaner")]
    public class CleanerTests
    {
        [Test]
        public void Clean_whenTheRemoteRootContainsAFileThatIsNotInTheLocalRoot_shouldCallTheRemoveTheRemoteFileAnTheRemoteNodeShouldNotBeInTheTreeAnymore()
        {
            // Given in the remote root there is a file that is not in the local root
            // then the system should delete the remote file
            // and should remove the corresponding node in the remote tree
            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Directory
                },
                Parent = null
            };

            var childFileTreeNode = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode {
                    Id = "2",
                    Name = "RemoteFileNotInLocal_[[2016-1-1-0-0-0]].jpeg",
                    Type = NodeType.File
                }
            };

            remoteTreeRoot.AddChild(childFileTreeNode);

            var localRoot = new LocalNode
            {
                Name = "LocalRoot",
                Type = NodeType.Directory
            };

            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockClient.Setup(m => m.Delete(childFileTreeNode.ObjectValue, true)).Verifiable();

            var mockLogger = new Mock<ILogger>();

            // Act
            var cleaner = new Cleaner(mockClient.Object);
            cleaner.CleanUp(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            remoteTreeRoot.ChildNodes.Should().NotContain(node => node.ObjectValue.Id == childFileTreeNode.ObjectValue.Id);
        }

        [Test]
        public void Clean_whenRemoteRootContainsAFileThatIsInTheLocalRoot_thenTheClientShouldNotDeleteTheFile()
        {
            // Given in the remote root there is a file that is in the local root
            // then the system should not delete the remote file
            // and should not remove the corresponding node in the remote tree
            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Directory
                },
                Parent = null
            };

            var remoteChildFileTreeNode = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "RemoteFileInLocal_[[2016-1-1-0-0-0]].jpeg",
                    Type = NodeType.File,
                    Size = 100,
                    LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
                }
            };

            remoteTreeRoot.AddChild(remoteChildFileTreeNode);

            var localRoot = new LocalNode
            {
                Name = "LocalRoot",
                Type = NodeType.Directory
            };

            var localChildFile = new LocalNode
            {
                Name = "RemoteFileInLocal.jpeg",
                Size = remoteChildFileTreeNode.ObjectValue.Size,
                LastModificationDate = remoteChildFileTreeNode.ObjectValue.LastModificationDate,
                Type = remoteChildFileTreeNode.ObjectValue.Type
            };

            localRoot.AddChild(localChildFile);

            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            var mockLogger = new Mock<ILogger>();

            // Act
            var cleaner = new Cleaner(mockClient.Object);
            cleaner.CleanUp(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            remoteTreeRoot.ChildNodes.Should().Contain(node => node.ObjectValue.Id == remoteChildFileTreeNode.ObjectValue.Id);
        }

        [Test]
        public void Clean_whenTheRemoteRootContainsAFolderThatIsNotInTheLocalRoot_shouldCallTheRemoveTheRemoteFolderAnTheRemoteNodeShouldNotBeInTheTreeAnymore()
        {
            // Given in the remote root there is a folder that is not in the local root
            // then the system should delete the remote folder
            // and should remove the corresponding node in the remote tree
            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Directory
                },
                Parent = null
            };

            var childFolderTreeNode = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "RemoteFolderNotInLocal",
                    Type = NodeType.Directory
                }
            };

            remoteTreeRoot.AddChild(childFolderTreeNode);

            var localRoot = new LocalNode
            {
                Name = "LocalRoot",
                Type = NodeType.Directory
            };

            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockClient.Setup(m => m.Delete(childFolderTreeNode.ObjectValue, true)).Verifiable();

            var mockLogger = new Mock<ILogger>();

            // Act
            var cleaner = new Cleaner(mockClient.Object);
            cleaner.CleanUp(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            remoteTreeRoot.ChildNodes.Should().NotContain(node => node.ObjectValue.Id == childFolderTreeNode.ObjectValue.Id);
        }

        [Test]
        public void Clean_whenRemoteRootContainsAFolderThatIsInTheLocalRoot_thenTheClientShouldNotDeleteTheFolder()
        {
            // Given in the remote root there is a folder that is in the local root
            // then the system should not delete the remote folder
            // and should not remove the corresponding node in the remote tree
            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Directory,
                    LastModificationDate = new DateTime(2016, 1, 1)
                },
                Parent = null
            };

            var remoteChildFolderTreeNode = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    Name = "RemoteFolderInLocal",
                    Type = NodeType.Directory,
                    LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
                }
            };

            remoteTreeRoot.AddChild(remoteChildFolderTreeNode);

            var localRoot = new LocalNode
            {
                Name = "LocalRoot",
                Type = NodeType.Directory
            };

            var localChildFolder = new LocalNode
            {
                Name = "RemoteFolderInLocal",
                LastModificationDate = remoteChildFolderTreeNode.ObjectValue.LastModificationDate,
                Type = remoteChildFolderTreeNode.ObjectValue.Type
            };

            localRoot.AddChild(localChildFolder);

            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);

            var mockLogger = new Mock<ILogger>();

            // Act
            var cleaner = new Cleaner(mockClient.Object);
            cleaner.CleanUp(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            remoteTreeRoot.ChildNodes.Should().Contain(node => node.ObjectValue.Id == remoteChildFolderTreeNode.ObjectValue.Id);
        }
    }
}
