using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using Mirror2MegaNZ.DomainModel.Commands;
using Mirror2MegaNZ.Logic;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using System.IO;
using NLog;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    public class CommandExecutorTest
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();

        [Test]
        public void Execute_withAnUploadCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            // Arrange
            const string filename = "File1.jpeg";
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var sourcePath = @"c:\testing\" + filename;
            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = sourcePath,
                    DestinationPath = @"\",
                    LastModifiedDate = lastModifiedDate
                }
            };

            // the initial MegaNZ node list
            const string rootName = @"\";
            const string rootPath = @"\";
            const string rootMegaNzId = "0";
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns(rootMegaNzId);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            var remoteItems = new List<MegaNzItem> {
                new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, rootName, ItemType.Folder, rootPath, 0)
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            // The name of the uploaded file
            var uploadedFileName = NameHandler.BuildRemoteFileName(filename, lastModifiedDate);

            var mockProgressNotifier = new Mock<IProgress<double>>();

            const string newFileMegaNzId = "1";
            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.UploadAsync(It.IsAny<Stream>(), 
                                                  uploadedFileName,     // The name will contain the last modified date 
                                                  It.Is<INode>(node => node.Id == rootMegaNzId),
                                                  mockProgressNotifier.Object))
                             .ReturnsAsync(new MegaNzNodeMock {
                                 Id = newFileMegaNzId,
                                 Name = uploadedFileName,
                                 ParentId = rootMegaNzId,
                                 Size = 1024,
                                 Type = NodeType.File,
                                 LastModificationDate = lastModifiedDate });

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(sourcePath)).Returns((FileStream)null);

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            executor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            mockMegaApiClient.VerifyAll();
            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockFileManager.VerifyAll();
        }

        [Test]
        public void Execute_withAnUploadCommand_shouldUploadTheMegaNzItemCollection()
        {
            // Arrange
            const string filename = "File1.jpeg";
            var sourcePath = @"c:\testing\" + filename;
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);

            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = sourcePath,
                    DestinationPath = @"\",
                    LastModifiedDate = lastModifiedDate
                }
            };

            // the initial MegaNZ node list
            const string rootName = @"\";
            const string rootPath = @"\";
            const string rootMegaNzId = "0";
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns(rootMegaNzId);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            var remoteItems = new List<MegaNzItem> {
                new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, rootName, ItemType.Folder, rootPath, 0)
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            const string newFileMegaNzId = "1";
            var uploadResultNode = new MegaNzNodeMock
            {
                Id = newFileMegaNzId,
                Name = NameHandler.BuildRemoteFileName(filename, lastModifiedDate),
                ParentId = rootMegaNzId,
                Size = 1024,
                Type = NodeType.File,
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            // The name of the uploaded file
            var uploadedFileName = NameHandler.BuildRemoteFileName(filename, lastModifiedDate);

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.UploadAsync(It.IsAny<Stream>(),
                                                       uploadedFileName,
                                                       It.Is<INode>(node => node.Id == rootMegaNzId),
                                                       mockProgressNotifier.Object))
                             .ReturnsAsync(uploadResultNode);

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(sourcePath)).Returns((FileStream)null);

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            executor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            megaNzItemCollection.GetList().Should().Contain(item => item.MegaNzNode.Id == newFileMegaNzId);
        }

        [Test]
        public void Execute_withADeleteCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            // Arrange
            var deleteCommand = new DeleteFileCommand
            {
                PathToDelete = "\\folder1\\file1.jpeg",
                LastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };
            var commandList = new[] { deleteCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // File1.jpeg in the remote file structure
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Id).Returns("2");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Name).Returns("file1.jpeg");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.ParentId).Returns("1");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Type).Returns(NodeType.File);
            var megaNzItemForRemoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, "\\folder1\\file1.jpeg", ItemType.File, "\\folder1\\file1.jpeg", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1,
                megaNzItemForRemoteFile1
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Delete(mockMegaNzNodeForRemoteFile1.Object, true)).Verifiable();

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            mockMegaApiClient.VerifyAll();
        }

        [Test]
        public void Execute_withADeleteCommand_shouldRemoveTheDeletedItemFromTheMegaNzItemCollection()
        {
            // Arrange
            var deleteCommand = new DeleteFileCommand
            {
                PathToDelete = "\\folder1\\file1.jpeg",
                LastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };
            var commandList = new[] { deleteCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // File1.jpeg in the remote file structure
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Id).Returns("2");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Name).Returns("file1.jpeg");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.ParentId).Returns("1");
            mockMegaNzNodeForRemoteFile1.SetupGet(m => m.Type).Returns(NodeType.File);
            var megaNzItemForRemoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, "\\folder1\\file1.jpeg", ItemType.File, "\\folder1\\file1.jpeg", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1,
                megaNzItemForRemoteFile1
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Delete(mockMegaNzNodeForRemoteFile1.Object, true)).Verifiable();

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            Action action = () => megaNzItemCollection.GetByPath("\\folder1\\file1.jpeg");
            action.ShouldThrow<KeyNotFoundException>("the key is not in the dictionary");
        }

        [Test]
        public void Execute_withACreateFolderCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            // Scenario
            // Local file structure
            // root
            //    |
            //    +--> folder1
            //               |
            //               +--> folder2
            //
            // Remote file structure
            // root
            //    |
            //    +--> folder1
            
            // Arrange
            var createFolderCommand = new CreateFolderCommand {
                ParentPath = "\\folder1",
                Name = "folder2"
            };
            var commandList = new[] { createFolderCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            // The MegaNzNode for the folder created
            var mockMegaNzNodeForRemoteFolder2 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Id).Returns("2");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Name).Returns("folder2");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.ParentId).Returns("1");

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient
                .Setup(m => m.CreateFolder(createFolderCommand.Name, mockMegaNzNodeForRemoteFolder1.Object))
                .Returns(mockMegaNzNodeForRemoteFolder2.Object);

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            mockMegaApiClient.VerifyAll();
        }

        [Test]
        public void Execute_withACreateFolderCommand_shouldUploadTheMegaNzItemCollection()
        {
            // Scenario
            // Local file structure
            // root
            //    |
            //    +--> folder1
            //               |
            //               +--> folder2
            //
            // Remote file structure
            // root
            //    |
            //    +--> folder1

            // Arrange
            var createFolderCommand = new CreateFolderCommand
            {
                ParentPath = "\\folder1",
                Name = "folder2"
            };
            var commandList = new[] { createFolderCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            // The MegaNzNode for the folder created
            var mockMegaNzNodeForRemoteFolder2 = new MegaNzNodeMock {
                Id = "2",
                Name = "folder2",
                Type = NodeType.Directory,
                Size = 0,
                ParentId = "1"
            };

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient
                .Setup(m => m.CreateFolder(createFolderCommand.Name, mockMegaNzNodeForRemoteFolder1.Object))
                .Returns(mockMegaNzNodeForRemoteFolder2);

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            megaNzItemCollection.GetList().Should().Contain(item => item.MegaNzNode.Id == mockMegaNzNodeForRemoteFolder2.Id);
        }

        [Test]
        public void Execute_withADeleteFolderCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            // Scenario
            // Local file structure
            // root
            //    |
            //    +--> folder1
            //
            // Remote file structure
            // root
            //    |
            //    +--> folder1
            //               |
            //               +--> folder2

            // Arrange
            var deleteFolderCommand = new DeleteFolderCommand
            {
                PathToDelete = "\\folder1\\folder2"
            };
            var commandList = new[] { deleteFolderCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // Folder2 in the remote file structure
            var mockMegaNzNodeForRemoteFolder2 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Id).Returns("2");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.ParentId).Returns("1");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Name).Returns("folder2");
            var megaNzItemForRemoteFolder2 = new MegaNzItem(mockMegaNzNodeForRemoteFolder2.Object, "folder2", ItemType.Folder, "\\folder1\\folder2", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1,
                megaNzItemForRemoteFolder2
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient
                .Setup(m => m.Delete(mockMegaNzNodeForRemoteFolder2.Object, true))
                .Verifiable();

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            mockMegaApiClient.VerifyAll();
        }

        [Test]
        public void Execute_withADeleteFolderCommand_shouldRemoteTheDeletedItemFromTheMegaNzItemCollection()
        {
            // Scenario
            // Local file structure
            // root
            //    |
            //    +--> folder1
            //
            // Remote file structure
            // root
            //    |
            //    +--> folder1
            //               |
            //               +--> folder2

            // Arrange
            var deleteFolderCommand = new DeleteFolderCommand
            {
                PathToDelete = "\\folder1\\folder2"
            };
            var commandList = new[] { deleteFolderCommand };

            // Remote items
            // Root in the remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Id).Returns("0");
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Type).Returns(NodeType.Root);
            mockMegaNzNodeForRemoteRoot.SetupGet(m => m.Name).Returns("\\");
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Id).Returns("1");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.ParentId).Returns("0");
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder1.SetupGet(m => m.Name).Returns("folder1");
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // Folder2 in the remote file structure
            var mockMegaNzNodeForRemoteFolder2 = new Mock<INode>(MockBehavior.Strict);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Id).Returns("2");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.ParentId).Returns("1");
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Type).Returns(NodeType.Directory);
            mockMegaNzNodeForRemoteFolder2.SetupGet(m => m.Name).Returns("folder2");
            var megaNzItemForRemoteFolder2 = new MegaNzItem(mockMegaNzNodeForRemoteFolder2.Object, "folder2", ItemType.Folder, "\\folder1\\folder2", 0);

            var remoteItems = new[] {
                megaNzItemRemoteRoot,
                megaNzItemForRemoteFolder1,
                megaNzItemForRemoteFolder2
            };
            var megaNzItemCollection = new MegaNzItemCollection(remoteItems);

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient
                .Setup(m => m.Delete(mockMegaNzNodeForRemoteFolder2.Object, true))
                .Verifiable();

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);

            var mockProgressNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object, _mockLogger.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            megaNzItemCollection.GetList().Should().NotContain(item => item.MegaNzNode.Id == "2");
        }
    }
}
