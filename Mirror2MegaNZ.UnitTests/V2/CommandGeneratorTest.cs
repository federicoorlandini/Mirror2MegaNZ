using Mirror2MegaNZ.V2.Logic;
using Mirror2MegaNZ.V2.DomainModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using Mirror2MegaNZ.V2.DomainModel.Commands;

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
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem("File1.jpeg", ItemType.File, @"\File1.jpeg", 1024, new DateTime(2017, 1, 1, 0, 0, 0));
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
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
            uploadCommand.SourcePath = localBasePath + localFile1.Path;
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
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFile1 = new MegaNzItem("1", "File1.jpeg", ItemType.File, @"\\File1.jpeg", 1024, new DateTime(2017, 1, 1, 0, 0, 0));
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
            var lastMofification = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastMofification);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + fileName, size, lastMofification);
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
            var lastMofification = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastMofification);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + fileName, size * 2, lastMofification);
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

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
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
            var lastMofification = new DateTime(2017, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastMofification);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + fileName, size * 2, lastMofification.AddYears(-1));
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

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
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
            var lastMofification = new DateTime(2015, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastMofification);
            var localItems = new List<FileItem> {
                localRoot,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + fileName, size * 2, lastMofification.AddYears(1));
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

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.DestinationPath.Should().Be(remoteRoot.Path);
            uploadCommand.SourcePath.Should().Be(localBasePath.TrimEnd('\\') + localFile1.Path);
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
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
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
            const string megaNzIdForRemoteRoot = "0";
            const string folderName = "Folder1";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0);
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
        }

        [Test]
        public void GenerateCommandList_withAFolderInTheLocalRootThatIsInTheRemoteRoot_shouldNotGenerateAnyCommand()
        {
            // Given the following local file structure:
            // root
            //    |
            //    +--> Folder1 - Last modified 2016-01-01-00-00-00
            //
            // And given the following remote file structure:
            // root
            //    |
            //    +--> Folder1 - Last modified 2016-01-01-00-00-00
            //
            // Then the generated command list should be empty

            // Local file structure
            const string folderName = "Folder1";
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
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
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localFile1 = new FileItem("File1.jpeg", ItemType.File, @"\" + folderName + @"\File1.jpeg", 1024, new DateTime(2016, 1, 1, 0, 0, 0));
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
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
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var remoteFile1 = new MegaNzItem("2", "File1.jpeg", ItemType.File, @"\" + folderName + @"\File1.jpeg", 1024, new DateTime(2016, 1, 1, 0, 0, 0));
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
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + fileName, size, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + fileName, size, lastModified);
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
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size * 2, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModified);
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

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.SourcePath.Should().Be(@"c:\testing\Folder1\File1.jpeg");
            uploadCommand.DestinationPath.Should().Be(@"\Folder1\");
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
            var lastModified = new DateTime(2017, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModified.AddYears(-1));
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

            commandList[1].Should().BeOfType<UploadFileCommand>();
            var uploadCommand = (UploadFileCommand)commandList[1];
            uploadCommand.SourcePath.Should().Be(@"c:\testing\Folder1\File1.jpeg");
            uploadCommand.DestinationPath.Should().Be(@"\Folder1\");
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
            var lastModified = new DateTime(2016, 1, 1, 0, 0, 0);

            // Local file structure
            var localRoot = new FileItem(@"\", ItemType.Folder, @"\", 0);
            var localFolder1 = new FileItem(folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var localFile1 = new FileItem(fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModified);
            var localItems = new List<FileItem> {
                localRoot,
                localFolder1,
                localFile1
            };

            // Remote file structure
            const string megaNzIdForRemoteRoot = "0";
            var remoteRoot = new MegaNzItem(megaNzIdForRemoteRoot, @"\", ItemType.Folder, @"\", 0);
            var remoteFolder1 = new MegaNzItem("1", folderName, ItemType.Folder, @"\" + folderName, 0, lastModified);
            var remoteFile1 = new MegaNzItem("1", fileName, ItemType.File, @"\" + folderName + @"\" + fileName, size, lastModified);
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
        }
    }
}
