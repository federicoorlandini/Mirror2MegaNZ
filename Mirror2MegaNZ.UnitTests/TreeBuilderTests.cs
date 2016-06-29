using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Mirror2MegaNZ.DomainModel;
using Mirror2MegaNZ.Logic;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    public class TreeBuilderTests
    {
        [Test]
        public void Build_withAListOfATwoLevelHierarchy_shouldBuildTheCorrectTree()
        {
            // Given the following list of nodes:
            // - Node ID = 1 (root folder)
            // - Node ID = 2, ParentID = 1 (folder)
            // - Node ID = 3, ParentID = 1
            // - Node ID = 4, ParentID = 2
            // - Node ID = 5, ParentID = 2
            // should build the correct tree

            // Arrange
            var node1 = new MegaNZNode {
                Id = "1",
                Name = "RootNode",
                ParentId = null,
                Type = CG.Web.MegaApiClient.NodeType.Root
            };

            var node2 = new MegaNZNode
            {
                Id = "2",
                Name = "RootChild1",
                ParentId = "1",
                Type = CG.Web.MegaApiClient.NodeType.Directory
            };

            var node3 = new MegaNZNode
            {
                Id = "3",
                Name = "RootChild2",
                ParentId = "1",
                Type = CG.Web.MegaApiClient.NodeType.File
            };

            var node4 = new MegaNZNode
            {
                Id = "4",
                Name = "ChildNode2",
                ParentId = "2",
                Type = CG.Web.MegaApiClient.NodeType.File
            };

            var node5 = new MegaNZNode
            {
                Id = "5",
                Name = "ChildNode2",
                ParentId = "2",
                Type = CG.Web.MegaApiClient.NodeType.File
            };

            var nodeCollection = new List<MegaNZNode> { node1, node2, node3, node4, node5 };

            // Act
            var treeBuilder = new TreeBuilder();
            var root = treeBuilder.Build(nodeCollection);

            // Assert
            // Node 1 must be the root
            root.ObjectValue.Id.Should().Be(node1.Id);
            root.ChildNodes.Should().Contain(node => node.ObjectValue.Id == node2.Id).And.Contain(node => node.ObjectValue.Id == node3.Id);
            var nodeTree2 = root.ChildNodes.Single(node => node.ObjectValue.Id == node2.Id);
            nodeTree2.Parent.ObjectValue.Id.Should().Be(root.ObjectValue.Id);
            nodeTree2.ChildNodes.Should().Contain(node => node.ObjectValue.Id == node4.Id).And.Contain(node => node.ObjectValue.Id == node5.Id);

            var nodeTree3 = root.ChildNodes.Single(node => node.ObjectValue.Id == node3.Id);
            nodeTree3.Parent.ObjectValue.Id.Should().Be(root.ObjectValue.Id);
            nodeTree3.ChildNodes.Should().BeEmpty();

            var nodeTree4 = nodeTree2.ChildNodes.Single(node => node.ObjectValue.Id == node4.Id);
            nodeTree4.Parent.ObjectValue.Id.Should().Be(nodeTree2.ObjectValue.Id);
            nodeTree4.ChildNodes.Should().BeEmpty();

            var nodeTree5 = nodeTree2.ChildNodes.Single(node => node.ObjectValue.Id == node5.Id);
            nodeTree5.Parent.ObjectValue.Id.Should().Be(nodeTree2.ObjectValue.Id);
            nodeTree5.ChildNodes.Should().BeEmpty();
        }
    }
}
