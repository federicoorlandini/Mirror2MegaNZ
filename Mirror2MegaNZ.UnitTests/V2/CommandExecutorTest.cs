using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using Mirror2MegaNZ.V2.Logic;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

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
            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = @"c:\testing\" + filename,
                    DestinationPath = @"\"
                }
            };

            // the initial MegaNZ node list
            const string rootName = @"\";
            const string rootPath = @"\";
            const string rootMegaNzId = "0";
            var remoteItems = new List<MegaNzItem> {
                new MegaNzItem(rootMegaNzId, rootName, ItemType.Folder, rootPath, 0)
            };

            const string newFileMegaNzId = "1";
            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Upload(It.IsAny<System.IO.Stream>(), 
                                                  filename, 
                                                  It.Is<INode>(node => node.Id == rootMegaNzId)))
                             .Returns(new MegaNzNodeMock {
                                 Id = newFileMegaNzId,
                                 Name = filename,
                                 ParentId = rootMegaNzId,
                                 Size = 1024,
                                 Type = NodeType.File,
                                 LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0) });

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object, remoteItems);
            executor.Execute(commandList);

            // Assert
            mockMegaApiClient.VerifyAll();
        }

        [Test]
        public void Execute_withAnUploadCommand_shouldUploadTheMegaNzItemCollection()
        {
            // Arrange
            const string filename = "File1.jpeg";
            // The command list to be executed
            var commandList = new ICommand[] {
                new UploadFileCommand() {
                    SourcePath = @"c:\testing\" + filename,
                    DestinationPath = @"\"
                }
            };

            // the initial MegaNZ node list
            const string rootName = @"\";
            const string rootPath = @"\";
            const string rootMegaNzId = "0";
            var remoteItems = new List<MegaNzItem> {
                new MegaNzItem(rootMegaNzId, rootName, ItemType.Folder, rootPath, 0)
            };

            const string newFileMegaNzId = "1";
            var mockMegaApiClient = new Mock<IMegaApiClient>(MockBehavior.Strict);
            mockMegaApiClient.Setup(m => m.Upload(It.IsAny<System.IO.Stream>(),
                                                  filename,
                                                  It.Is<INode>(node => node.Id == rootMegaNzId)))
                             .Returns(new MegaNzNodeMock
                             {
                                 Id = newFileMegaNzId,
                                 Name = filename,
                                 ParentId = rootMegaNzId,
                                 Size = 1024,
                                 Type = NodeType.File,
                                 LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
                             });

            // Act
            var executor = new CommandExecutor(mockMegaApiClient.Object, remoteItems);
            executor.Execute(commandList);

            // Assert
            executor.MegaNzItems.Should().Contain(item => item.MegaNzId == newFileMegaNzId);
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
