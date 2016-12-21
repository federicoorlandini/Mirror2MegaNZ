using CG.Web.MegaApiClient;
using FluentAssertions;
using Mirror2MegaNZ.V2.DomainModel;
using Mirror2MegaNZ.V2.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class MegaNzItemListGeneratorTests
    {
        [Test]
        public void Generate_shouldReturnTheCorrectCollectionOfItems()
        {
            // Given the following file tree in MegaNZ
            // root
            //    |
            //    +--> Folder1
            //    |          |
            //    |          +--> File1_[[2016-1-1-0-0-0]].jpeg
            //    |
            //    +--> File2_[[2017-2-2-1-1-1]].jpeg
            //
            // The result collection should contain:
            // - / (root)
            // - /Folder1/
            // - /Folder1/File1.jpeg (with last modification date = 1st Gen 2016)
            // - /File2.jpeg (with last modification date = 2st Feb 2016 01:01:01)
            var root = new MegaNzNodeMock {
                Id = "1",
                Name = @"\",
                ParentId = string.Empty,
                Type = NodeType.Root
            };

            var folder1 = new MegaNzNodeMock
            {
                Id = "2",
                Name = "Folder1",
                ParentId = root.Id,
                Type = NodeType.Directory
            };

            var file1 = new MegaNzNodeMock
            {
                Id = "3",
                Name = "File1_[[2016-1-1-0-0-0]].jpeg",
                ParentId = folder1.Id,
                Type = NodeType.File,
                Size = 1024
            };

            var file2 = new MegaNzNodeMock
            {
                Id = "4",
                Name = "File2_[[2017-2-2-1-1-1]].jpeg",
                ParentId = root.Id,
                Type = NodeType.File,
                Size = 2048
            };

            var megaNzCollection = new List<INode> {
                root,
                folder1,
                file1,
                file2
            };

            // Act
            var generator = new MegaNzItemListGenerator();
            MegaNzItem[] result = generator.Generate(megaNzCollection).ToArray();

            // Assert
            result.Length.Should().Be(4);

            result[0].Name.Should().Be(@"\");
            result[0].Path.Should().Be(@"\");
            result[0].Type.Should().Be(ItemType.Folder);

            result[1].Name.Should().Be("Folder1");
            result[1].Path.Should().Be(@"\Folder1\");  // Forlder's path MUST end with a backslash
            result[1].Type.Should().Be(ItemType.Folder);

            result[2].Name.Should().Be("File1.jpeg");
            result[2].Path.Should().Be(@"\Folder1\File1.jpeg");
            result[2].LastModified.Should().HaveValue();
            result[2].LastModified.Value.Year.Should().Be(2016);
            result[2].LastModified.Value.Month.Should().Be(1);
            result[2].LastModified.Value.Day.Should().Be(1);
            result[2].LastModified.Value.Hour.Should().Be(0);
            result[2].LastModified.Value.Minute.Should().Be(0);
            result[2].LastModified.Value.Second.Should().Be(0);
            result[2].Type.Should().Be(ItemType.File);

            result[3].Name.Should().Be("File2.jpeg");
            result[3].Path.Should().Be(@"\File2.jpeg");
            result[3].LastModified.Should().HaveValue();
            result[3].LastModified.Value.Year.Should().Be(2017);
            result[3].LastModified.Value.Month.Should().Be(2);
            result[3].LastModified.Value.Day.Should().Be(2);
            result[3].LastModified.Value.Hour.Should().Be(1);
            result[3].LastModified.Value.Minute.Should().Be(1);
            result[3].LastModified.Value.Second.Should().Be(1);
            result[3].Type.Should().Be(ItemType.File);
        }

        [Test]
        public void Generate_shouldFilterOutTheTrashFolder()
        {
            var root = new MegaNzNodeMock
            {
                Id = "1",
                Name = @"\",
                ParentId = string.Empty,
                Type = NodeType.Root
            };

            var trashFolder = new MegaNzNodeMock
            {
                Id = "2",
                Name = null,
                ParentId = string.Empty,
                Type = NodeType.Trash
            };

            var megaNzCollection = new List<INode> {
                root,
                trashFolder
            };

            // Act
            var generator = new MegaNzItemListGenerator();
            MegaNzItem[] result = generator.Generate(megaNzCollection).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].MegaNzNode.Type.Should().Be(NodeType.Root);
        }

        [Test]
        public void Generate_shouldFilterOutTheInboxFolder()
        {
            var root = new MegaNzNodeMock
            {
                Id = "1",
                Name = @"\",
                ParentId = string.Empty,
                Type = NodeType.Root
            };

            var inboxFolder = new MegaNzNodeMock
            {
                Id = "2",
                Name = null,
                ParentId = string.Empty,
                Type = NodeType.Inbox
            };

            var megaNzCollection = new List<INode> {
                root,
                inboxFolder
            };

            // Act
            var generator = new MegaNzItemListGenerator();
            MegaNzItem[] result = generator.Generate(megaNzCollection).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].MegaNzNode.Type.Should().Be(NodeType.Root);
        }

        [Test]
        public void Generate_shouldFilterOutTheTrashFolderHierarchyNodes()
        {
            var root = new MegaNzNodeMock
            {
                Id = "1",
                Name = @"\",
                ParentId = string.Empty,
                Type = NodeType.Root
            };

            var trashFolder = new MegaNzNodeMock
            {
                Id = "2",
                Name = null,
                ParentId = string.Empty,
                Type = NodeType.Trash
            };

            var folderChildOfTrash = new MegaNzNodeMock
            {
                Id = "3",
                Name = "folder1",
                ParentId = trashFolder.Id,
                Type = NodeType.Directory
            };

            var fileChildOfTrash = new MegaNzNodeMock
            {
                Id = "4",
                Name = "file1",
                ParentId = folderChildOfTrash.Id,
                Type = NodeType.File,
                Size = 1024,
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var megaNzCollection = new List<INode> {
                root,
                trashFolder,
                folderChildOfTrash,
                fileChildOfTrash
            };

            // Act
            var generator = new MegaNzItemListGenerator();
            MegaNzItem[] result = generator.Generate(megaNzCollection).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].MegaNzNode.Type.Should().Be(NodeType.Root);
        }

        [Test]
        public void Generate_shouldFilterOutTheInboxFolderHierarchyNodes()
        {
            var root = new MegaNzNodeMock
            {
                Id = "1",
                Name = @"\",
                ParentId = string.Empty,
                Type = NodeType.Root
            };

            var inboxFolder = new MegaNzNodeMock
            {
                Id = "2",
                Name = null,
                ParentId = string.Empty,
                Type = NodeType.Trash
            };

            var folderChildOfInbox = new MegaNzNodeMock
            {
                Id = "3",
                Name = "folder1",
                ParentId = inboxFolder.Id,
                Type = NodeType.Directory
            };

            var fileChildOfInbox = new MegaNzNodeMock
            {
                Id = "4",
                Name = "file1",
                ParentId = folderChildOfInbox.Id,
                Type = NodeType.File,
                Size = 1024,
                LastModificationDate = new DateTime(2016, 1, 1, 0, 0, 0)
            };

            var megaNzCollection = new List<INode> {
                root,
                inboxFolder,
                folderChildOfInbox,
                fileChildOfInbox
            };

            // Act
            var generator = new MegaNzItemListGenerator();
            MegaNzItem[] result = generator.Generate(megaNzCollection).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].MegaNzNode.Type.Should().Be(NodeType.Root);
        }
    }
}
