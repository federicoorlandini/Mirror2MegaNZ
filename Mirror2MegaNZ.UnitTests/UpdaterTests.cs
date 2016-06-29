using NUnit.Framework;
using CG.Web.MegaApiClient;
using Moq;
using Mirror2MegaNZ.DomainModel;
using System;
using NLog;
using System.IO;
using Mirror2MegaNZ.Logic;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    [Category("Updater")]
    public class UpdaterTests
    {
        [Test]
        public void Update_whenInTheLocalNodeThereIsAFileThatIsNotInTheRemoteNode_shouldUploadTheFile()
        {
            // Given in the local root there is a file that is not in the remote root
            // then the system should upload the local file
            var localRoot = new LocalNode
            {
                Name = "LocalRoot",
                Type = NodeType.Directory
            };

            var localFileInRoot = new LocalNode
            {
                Name = "LocalFileNotInRemote.jpeg",
                Type = NodeType.File,
                FullPath = @"c:\somedirectory\LocalFileNotInRemote.jpeg",
                LastModificationDate = new DateTime(2016, 1 ,1 , 0, 0, 0)
            };

            localRoot.AddChild(localFileInRoot);

            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Directory
                },
                Parent = null
            };

            const string remoteFileName = "LocalFileNotInRemote-2016_1_1_0_0_0.jpeg";
            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockClient.Setup(m => m.Upload(It.IsAny<Stream>(), remoteFileName, remoteTreeRoot.ObjectValue)).Returns(new MegaNZNode());    // We don't care the result

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(localFileInRoot.FullPath)).Returns((FileStream)null);

            var mockLogger = new Mock<ILogger>();

            // Act
            var updater = new Updater(mockClient.Object, mockFileManager.Object);
            updater.Update(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            mockFileManager.VerifyAll();
        }

        [Test]
        public void Update_whenInTheLocalNodeThereIsAFolderThatIsNotInTheRemoteNode_shouldCreateTheFolderAndUploadAllTheFile()
        {
            // Given there is a folder in the local root that is not in the remote folder
            // And the local folder contains some files
            // Then the client.Create() must be colled to create the folder in remote
            // And all the file in the loca folder must be updated in the remote new folder
            // And in the remote root we should find a new folder with the correct parameters
            // And in the new folder in remote there should be two new files with the correct parameters
            var localRoot = new LocalNode
            {
                FullPath = @"c:\rootfolder\",
                Name = "LocalRoot",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Type = NodeType.Directory 
            };

            var localFolderInRoot = new LocalNode
            {
                FullPath = @"c:\rootfolder\first_folder",
                Name = "First folder",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Type = NodeType.Directory
            };
            localRoot.AddChild(localFolderInRoot);

            var file1InLocalFolder = new LocalNode
            {
                FullPath = @"c:\rootfolder\first_folder\file1.jpg",
                Name = "file1.jpg",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Type = NodeType.File
            };
            localFolderInRoot.AddChild(file1InLocalFolder);

            var file2InLocalFolder = new LocalNode
            {
                FullPath = @"c:\rootfolder\first_folder\file2.jpg",
                Name = "File2",
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0),
                Type = NodeType.File
            };
            localFolderInRoot.AddChild(file2InLocalFolder);

            var remoteTreeRoot = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "1",
                    Type = NodeType.Root
                }
            };

            var mockClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(file1InLocalFolder.FullPath)).Returns((FileStream)null);
            mockFileManager.Setup(m => m.GetStreamToReadFile(file2InLocalFolder.FullPath)).Returns((FileStream)null);

            var newRemoteFolderTreeNode = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "2",
                    LastModificationDate = localFolderInRoot.LastModificationDate,
                    Name = localFolderInRoot.Name,
                    Type = NodeType.Directory
                },
                Parent = remoteTreeRoot,
            };
            mockClient.Setup(m => m.CreateFolder(NameHandler.BuildRemoteFolderName(localFolderInRoot.Name), 
                                                 remoteTreeRoot.ObjectValue))
                      .Returns(newRemoteFolderTreeNode.ObjectValue);
            

            var newFile1InRemoteFolder = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "3",
                    LastModificationDate = file1InLocalFolder.LastModificationDate,
                    Name = file1InLocalFolder.Name,
                    Size = file1InLocalFolder.Size,
                    Type = NodeType.File
                },
                Parent = newRemoteFolderTreeNode
            };
            mockClient.Setup(m => m.Upload(It.IsAny<FileStream>(), 
                                           NameHandler.BuildRemoteFileName(file1InLocalFolder.Name, file1InLocalFolder.LastModificationDate), 
                                           It.Is<INode>(node => node.Id == newRemoteFolderTreeNode.ObjectValue.Id)))
                      .Returns(newFile1InRemoteFolder.ObjectValue);

            var newFile2InRemoteFolder = new MegaNZTreeNode
            {
                ObjectValue = new MegaNZNode
                {
                    Id = "4",
                    LastModificationDate = file2InLocalFolder.LastModificationDate,
                    Name = file2InLocalFolder.Name,
                    Size = file2InLocalFolder.Size,
                    Type = NodeType.File
                },                
                Parent = newRemoteFolderTreeNode
            };
            mockClient.Setup(m => m.Upload(It.IsAny<FileStream>(),
                                           NameHandler.BuildRemoteFileName(file2InLocalFolder.Name, file2InLocalFolder.LastModificationDate), 
                                           It.Is<INode>(node => node.Id == newRemoteFolderTreeNode.ObjectValue.Id)))
                      .Returns(newFile2InRemoteFolder.ObjectValue);

            var mockLogger = new Mock<ILogger>();

            // Act
            var updater = new Updater(mockClient.Object, mockFileManager.Object);
            updater.Update(localRoot, remoteTreeRoot, mockLogger.Object);

            // Assert
            mockClient.VerifyAll();
            mockFileManager.VerifyAll();
        }
    }
}
