using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using Mirror2MegaNZ.V2.Logic;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using Mirror2MegaNZ.Logic;
using System.IO;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class CommandExecutorTest
    {
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
                                 Name = filename,
                                 ParentId = rootMegaNzId,
                                 Size = 1024,
                                 Type = NodeType.File,
                                 LastModificationDate = lastModifiedDate });

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(sourcePath)).Returns((FileStream)null);

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object);
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
            var executor = new CommandExecutor(mockMegaApiClient.Object);
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
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // File1.jpeg in the remote file structure
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
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
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object);
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
            var megaNzItemRemoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, "\\", ItemType.Folder, "\\", 0);

            // Folder1 in the remote file structure
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var megaNzItemForRemoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, "\\folder1", ItemType.Folder, "\\folder1", 0);

            // File1.jpeg in the remote file structure
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
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
            var commandExecutor = new CommandExecutor(mockMegaApiClient.Object);
            commandExecutor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgressNotifier.Object);

            // Assert
            Action action = () => megaNzItemCollection.GetByPath("\\folder1\\file1.jpeg");
            action.ShouldThrow<KeyNotFoundException>("the key is not in the dictionary");
        }

        [Test]
        public void Execute_withACreateFolderCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Execute_withACreateFolderCommand_shouldUploadTheMegaNzItemCollection()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Execute_withADeleteFolderCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Execute_withADeleteFolderCommand_shouldUploadTheMegaNzItemCollection()
        {
            throw new NotImplementedException();
        }
    }
}
