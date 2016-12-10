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
            var sourcePath = @"c:\testing\" + filename;
            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = sourcePath,
                    DestinationPath = @"\"
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
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var uploadedFileName = NameHandler.BuildRemoteFileName(filename, lastModifiedDate);

            const string newFileMegaNzId = "1";
            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Upload(It.IsAny<Stream>(), 
                                                  uploadedFileName,     // The name will contain the last modified date 
                                                  It.Is<INode>(node => node.Id == rootMegaNzId)))
                             .Returns(new MegaNzNodeMock {
                                 Id = newFileMegaNzId,
                                 Name = filename,
                                 ParentId = rootMegaNzId,
                                 Size = 1024,
                                 Type = NodeType.File,
                                 LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0) });

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(sourcePath)).Returns((FileStream)null);

            var mockProgrssNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object);
            executor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgrssNotifier.Object);

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

            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = sourcePath,
                    DestinationPath = @"\"
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

            const string newFileMegaNzId = "1";
            var uploadResultNode = new MegaNzNodeMock
            {
                Id = newFileMegaNzId,
                Name = filename,
                ParentId = rootMegaNzId,
                Size = 1024,
                Type = NodeType.File,
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Upload(It.IsAny<Stream>(),
                                                  filename,
                                                  It.Is<INode>(node => node.Id == rootMegaNzId)))
                             .Returns(uploadResultNode);

            var mockFileManager = new Mock<IFileManager>(MockBehavior.Strict);
            mockFileManager.Setup(m => m.GetStreamToReadFile(sourcePath)).Returns((FileStream)null);

            var mockProgrssNotifier = new Mock<IProgress<double>>(MockBehavior.Strict);

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object);
            executor.Execute(commandList, megaNzItemCollection, mockFileManager.Object, mockProgrssNotifier.Object);

            // Assert
            executor.MegaNzItems.Should().Contain(item => item.MegaNzNode.Id == newFileMegaNzId);
        }

        [Test]
        public void Execute_withADeleteCommand_shouldMakeTheCorrectCallToTheMegaNzClient()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Execute_withADeleteCommand_shouldUploadTheMegaNzItemCollection()
        {
            throw new NotImplementedException();
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
