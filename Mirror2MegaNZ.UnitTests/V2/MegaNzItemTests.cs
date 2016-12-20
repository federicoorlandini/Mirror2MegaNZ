using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;

namespace Mirror2MegaNZ.UnitTests.V2
{
    [TestFixture]
    [Category("V2")]
    public class MegaNzItemTests
    {
        private readonly MegaNzNodeMock _root = new MegaNzNodeMock
        {
            Id = "1",
            Name = "rootNode",
            Owner = "User",
            ParentId = string.Empty,
            Size = 0,
            Type = NodeType.Root
        };

        [Test]
        public void Constructor_usingINodeOfTypeRoot_shouldBuildTheCorrectPath()
        {
            // Arrange
            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root }
            };

            // Act
            var rootItem = new MegaNzItem(_root, nodeDictionary);

            // Assert
            rootItem.Path.Should().Be(@"\");
        }

        [Test]
        public void Constructor_usingINodeOfTypeRoot_shouldHaveTheCorrectName()
        {
            // Arrange
            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root }
            };

            // Act
            var rootItem = new MegaNzItem(_root, nodeDictionary);

            // Assert
            rootItem.Name.Should().Be(@"\");
        }

        [Test]
        public void Constructor_usingINodeOfTypeRoot_shouldHaveSizeZero()
        {
            // Arrange
            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root }
            };

            // Act
            var rootItem = new MegaNzItem(_root, nodeDictionary);

            // Assert
            rootItem.Size.Should().Be(0);
        }

        [Test]
        public void Constructor_usingINodeOfTypeRoot_shouldHaveTheCorrectType()
        {
            // Arrange
            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root }
            };

            // Act
            var rootItem = new MegaNzItem(_root, nodeDictionary);

            // Assert
            rootItem.Type.Should().Be(ItemType.Folder);
        }

        [Test]
        public void Constructor_usingINodeOfTypeDirectory_shouldBuildTheCorrectPath()
        {
            // We need a structure like this:
            // root
            //    +--> folder1
            //               +--> folder2
            //                          +--> folder3
            var folder1 = new MegaNzNodeMock
            {
                Id = "2",
                Name = "folder1",
                Owner = "User",
                ParentId = "1",
                Size = 0,
                Type = NodeType.Directory
            };

            var folder2 = new MegaNzNodeMock
            {
                Id = "3",
                Name = "folder2",
                Owner = "User",
                ParentId = "2",
                Size = 0,
                Type = NodeType.Directory
            };

            var folder3 = new MegaNzNodeMock
            {
                Id = "4",
                Name = "folder3",
                Owner = "User",
                ParentId = "3",
                Size = 0,
                Type = NodeType.Directory
            };

            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root },
                { folder1.Id, folder1 },
                { folder2.Id, folder2 },
                { folder3.Id, folder3 }
            };

            // Act
            var folder3Item = new MegaNzItem(folder3, nodeDictionary);

            // Assert
            folder3Item.Path.Should().Be(@"\folder1\folder2\folder3");
        }

        [Test]
        public void Constructor_usingINodeOfTypeFile_shouldBuildTheCorrectPath()
        {
            // We need a structure like this:
            // root
            //    +--> folder1
            //               +--> folder2
            //                          +--> file1
            var folder1 = new MegaNzNodeMock
            {
                Id = "2",
                Name = "folder1",
                Owner = "User",
                ParentId = "1",
                Size = 0,
                Type = NodeType.Directory
            };

            var folder2 = new MegaNzNodeMock
            {
                Id = "3",
                Name = "folder2",
                Owner = "User",
                ParentId = "2",
                Size = 0,
                Type = NodeType.Directory
            };

            var file1 = new MegaNzNodeMock
            {
                Id = "4",
                Name = "file1_[[2016-1-1-0-0-1]].jpeg",
                Owner = "User",
                ParentId = "3",
                Size = 1024,
                Type = NodeType.File
            };

            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root },
                { folder1.Id, folder1 },
                { folder2.Id, folder2 },
                { file1.Id, file1 }
            };

            // Act
            var file1Item = new MegaNzItem(file1, nodeDictionary);

            // Assert
            file1Item.Path.Should().Be(@"\folder1\folder2\file1.jpeg");
        }

        [Test]
        public void Constructor_usingINodeOfTypeFile_shouldParseTheCorrectLastModificationDatetime()
        {
            // We need a structure like this:
            // root
            //    +--> folder1
            //               +--> folder2
            //                          +--> file1
            var folder1 = new MegaNzNodeMock
            {
                Id = "2",
                Name = "folder1",
                Owner = "User",
                ParentId = "1",
                Size = 0,
                Type = NodeType.Directory
            };

            var folder2 = new MegaNzNodeMock
            {
                Id = "3",
                Name = "folder2",
                Owner = "User",
                ParentId = "2",
                Size = 0,
                Type = NodeType.Directory
            };

            var file1 = new MegaNzNodeMock
            {
                Id = "4",
                Name = "file1_[[2016-1-2-3-4-5]].jpeg",
                Owner = "User",
                ParentId = "3",
                Size = 1024,
                Type = NodeType.File
            };

            var nodeDictionary = new Dictionary<string, INode> {
                { _root.Id, _root },
                { folder1.Id, folder1 },
                { folder2.Id, folder2 },
                { file1.Id, file1 }
            };

            // Act
            var file1Item = new MegaNzItem(file1, nodeDictionary);

            // Assert
            file1Item.LastModified.Should().HaveValue();
            file1Item.LastModified.Value.Year.Should().Be(2016);
            file1Item.LastModified.Value.Month.Should().Be(1);
            file1Item.LastModified.Value.Day.Should().Be(2);
            file1Item.LastModified.Value.Hour.Should().Be(3);
            file1Item.LastModified.Value.Minute.Should().Be(4);
            file1Item.LastModified.Value.Second.Should().Be(5);
        }
    }
}
