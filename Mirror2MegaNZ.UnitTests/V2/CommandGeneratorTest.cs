using Mirror2MegaNZ.V2.Logic;
using Mirror2MegaNZ.V2.DomainModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using Moq;
using CG.Web.MegaApiClient;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class CommandGeneratorTest
    {
        [Test]
        public void GenerateCommandList_withAFileInTheLocalRootThatIsNotInTheRemoteRoot_shouldGenerateAnUploadCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif 2016-1-1-0-0-0
            //
            // And given the following remote file structure:
            // root
            //    |
            //    (no file in the root)
            //
            // Then the generated command list should be:
            // - Upload \root\File1.jpeg

            // Local file structure
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem("File1.jpeg", ItemType.File, @"\File1.jpeg", 1024, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            var mockMegaNzNode = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNode.Object, @"\", ItemType.Folder, @"\", 0);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot
            };

            // Act
            var localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);
            commandList[0].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[0];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath + localFile1.Path);
            uploadCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNode.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInTheRemoteRootThatIsNotInTheLocalRoot_shouldGenerateADeleteCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    (no file in the root)
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif 2016-1-1-0-0-0
            //
            // Then the generated command list should be:
            // - Delete \File1.jpeg

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localItems = new List<FileItem> {
                localRoot
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            // File1
            var lastModifiedDate = new DateTime(2017, 1, 1, 0, 0, 0);
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, "File1.jpeg", ItemType.File, @"\\File1.jpeg", 1024, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFile1
            };

            // Act
            var localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);
            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(remoteFile1.Path);
            deleteCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInTheLocalRootThatIsTheSameInTheRemoteRoot_shouldNotGenerateAnyCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated an empty command list
            
            const string fileName = "File1.jpeg";
            const long size = 1024;
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            // File1
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, fileName, ItemType.File, @"\" + fileName, size, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Should().BeEmpty();

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInTheLocalRootThatHasDifferentSizeThanTheSameInTheRemoteRoot_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 2048
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \File1.jpeg
            // Upload \File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            // File1
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, fileName, ItemType.File, @"\" + fileName, size * 2, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(2);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(remoteFile1.Path);
            deleteCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
            uploadCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInTheLocalRootThatIsNewerThanTheSameInTheRemoteRoot_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2017-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \File1.jpeg
            // Upload \File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            var localLastModifiedDate = new DateTime(2017, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, localLastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            // File1
            var remoteLastModifiedDate = localLastModifiedDate.AddYears(-1);
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, fileName, ItemType.File, @"\" + fileName, size * 2, remoteLastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(2);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(remoteFile1.Path);
            deleteCommand.LastModifiedDate.Should().Be(remoteLastModifiedDate);

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
            uploadCommand.LastModifiedDate.Should().Be(localLastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInTheLocalRootThatIsOlderThanTheSameInTheRemoteRoot_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2015-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \File1.jpeg
            // Upload \File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            var localLastModifiedDate= new DateTime(2015, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, localLastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            // File1
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteLastModifiedDate = localLastModifiedDate.AddYears(1);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, fileName, ItemType.File, @"\" + fileName, size * 2, remoteLastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(2);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(remoteFile1.Path);
            deleteCommand.LastModifiedDate.Should().Be(remoteLastModifiedDate);

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
            uploadCommand.LastModifiedDate.Should().Be(localLastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFolderInTheLocalRootThatIsNotInTheRemoteRoot_shouldGenerateACreateFolderCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> (no folder)
            //
            // Then the generated command list should be:
            // CreateFolder \Folder1

            // Local file structure
            const string folderName = "folder1";
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, DateTime.Now);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);

            commandList[0].Should().BeOfType<CreateFolderCommand>();
            var createFolderCommand = (CreateFolderCommand)commandList[0];
            createFolderCommand.ParentPath.Should().Be(remoteRoot.Path);
            createFolderCommand.Name.Should().Be(folderName);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFolderInTheRemoteRootThatIsNotInTheLocalRoot_shouldGenerateADeleteFolderCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> (no folder)
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //
            // Then the generated command list should be:
            // DeleteFolder \Folder1
            
            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localItems = new List<FileItem> {
                localRoot
            };

            // Remote file structure
            // Root
            const string folderName = "Folder1";
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // File1
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);

            commandList[0].Should().BeOfType<DeleteFolderCommand>();
            var deleteFolderCommand = (DeleteFolderCommand)commandList[0];
            deleteFolderCommand.PathToDelete.Should().Be(remoteFolder1.Path);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFolderInTheLocalRootThatIsInTheRemoteRoot_shouldNotGenerateAnyCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //
            // Then the generated command list should be empty

            // Local file structure
            const string folderName = "Folder1";
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForFile1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var generator = new CommandGenerator(localBasePath);
            var commandList = generator.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Should().BeEmpty();

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheLocalRootThatIsNotInTheRemoteFolder_shouldGenerateAnUploadCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif 2016-1-1-0-0-0
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> (no file in the root)
            //
            // Then the generated command list should be:
            // - Upload \root\Folder1\File1.jpeg

            // Local file structure
            const string folderName = "Folder1";
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localFile1 = new FileItem("File1.jpeg", ItemType.File, @"\" + folderName + @"\File1.jpeg", 1024, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var generator = new CommandGenerator(localBasePath);
            var commandList = generator.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);

            commandList[0].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[0];
            uploadCommand.SourcePath.Should().Be(@"c:\testing\Folder1\File1.jpeg");
            uploadCommand.DestinationPath.Should().Be(@"\Folder1\");
            uploadCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForFolder1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheRemoteRootThatIsNotInTheLocalFolder_shouldGenerateADeleteCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif 2016-1-1-0-0-0
            //
            // Then the generated command list should be:
            // - Delete \Folder1\File1.jpeg

            // Local file structure
            const string folderName = "Folder1";
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);

            // File1
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, "File1.jpeg", ItemType.File, @"\" + folderName + @"\File1.jpeg", 1024, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var generator = new CommandGenerator(localBasePath);
            var commandList = generator.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(1);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(@"\Folder1\File1.jpeg");
            deleteCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForRemoteFolder1.VerifyAll();
            mockMegaNzNodeForRemoteFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheLocalRootThatIsTheSameInTheRemoteFolder_shouldNotGenerateAnyCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated an empty command list

            const string fileName = "File1.jpeg";
            const long size = 1024;
            const string folderName = "Folder1";
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);

            // File1
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, fileName, ItemType.File, @"\" + fileName, size, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Should().BeEmpty();

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForRemoteFolder1.VerifyAll();
            mockMegaNzNodeForRemoteFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheLocalRootThatHasDifferentSizeThanTheSameInTheRemoteFolder_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 2048
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \Folder1\File1.jpeg
            // Upload \Folder1\File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            const string folderName = "Folder1";
            var lastModifiedDate = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size * 2, lastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);

            // File1
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(2);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(@"\Folder1\File1.jpeg");
            deleteCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.SourcePath.Should().Be(@"c:\testing\Folder1\File1.jpeg");
            uploadCommand.DestinationPath.Should().Be(@"\Folder1\");
            uploadCommand.LastModifiedDate.Should().Be(lastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForRemoteFolder1.VerifyAll();
            mockMegaNzNodeForRemoteFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheLocalRootThatIsNewerThanTheSameInTheRemoteFolder_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2017-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \Folder1\File1.jpeg
            // Upload \Folder1\File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            const string folderName = "Folder1";
            var localLastModifiedDate = new DateTime(2017, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, localLastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);

            // File1
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteLastModifiedDate = localLastModifiedDate.AddYears(-1);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, remoteLastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Count.Should().Be(2);

            commandList[0].Should().BeOfType<DeleteFileCommand>();
            var deleteCommand = (DeleteFileCommand)commandList[0];
            deleteCommand.PathToDelete.Should().Be(@"\Folder1\File1.jpeg");
            deleteCommand.LastModifiedDate.Should().Be(remoteLastModifiedDate);

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.SourcePath.Should().Be(@"c:\testing\Folder1\File1.jpeg");
            uploadCommand.DestinationPath.Should().Be(@"\Folder1\");
            uploadCommand.LastModifiedDate.Should().Be(localLastModifiedDate);

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForRemoteFolder1.VerifyAll();
            mockMegaNzNodeForRemoteFile1.VerifyAll();
        }

        [Test]
        public void GenerateCommandList_withAFileInFolderInTheLocalRootThatIsOlderThanTheSameInTheRemoteFolder_shouldGenerateADeleteCommandAndAnUpdateCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2015-1-1-0-0-0 - Size = 1024
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1
            //               |
            //               +--> File1.jpeg - Last Modif = 2016-1-1-0-0-0 - Size = 1024
            //
            // Then the generated command list should be:
            // Delete \Folder1\File1.jpeg
            // Upload \Folder1\File1.jpeg

            const string fileName = "File1.jpeg";
            const long size = 1024;
            const string folderName = "Folder1";
            var localLastModifiedDate = new DateTime(2015, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, localLastModifiedDate);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            // Root
            var mockMegaNzNodeForRemoteRoot = new Mock<INode>(MockBehavior.Strict);
            var remoteRoot = new MegaNzItem(mockMegaNzNodeForRemoteRoot.Object, @"\", ItemType.Folder, @"\", 0);

            // Folder1
            var mockMegaNzNodeForRemoteFolder1 = new Mock<INode>(MockBehavior.Strict);
            var remoteFolder1 = new MegaNzItem(mockMegaNzNodeForRemoteFolder1.Object, folderName, ItemType.Folder, @"\" + folderName, 0);

            // File1
            var mockMegaNzNodeForRemoteFile1 = new Mock<INode>(MockBehavior.Strict);
            var remoteLastModifiedDate = localLastModifiedDate.AddYears(1);
            var remoteFile1 = new MegaNzItem(mockMegaNzNodeForRemoteFile1.Object, fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, remoteLastModifiedDate);
            var remoteItems = new List<MegaNzItem> {
                remoteRoot,
                remoteFolder1,
                remoteFile1
            };

            // Act
            const string localBasePath = "c:\\testing\\";
            var synchronizer = new CommandGenerator(localBasePath);
            var commandList = synchronizer.GenerateCommandList(localItems, remoteItems);

            // Assert
            commandList.Should().BeEmpty();

            mockMegaNzNodeForRemoteRoot.VerifyAll();
            mockMegaNzNodeForRemoteFolder1.VerifyAll();
            mockMegaNzNodeForRemoteFile1.VerifyAll();
        }
    }
}
