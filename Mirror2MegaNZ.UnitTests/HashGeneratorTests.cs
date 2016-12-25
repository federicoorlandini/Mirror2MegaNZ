using Mirror2MegaNZ.DomainModel;
using NUnit.Framework;
using System;

namespace Mirror2MegaNZ.UnitTests
{
    [TestFixture]
    public class HashGeneratorTests
    {
        [Test]
        public void Generate_theHashCodeForTwoEqualityObjects_shouldBeTheSame()
        {
            var megaNzNode = new MegaNZTreeNode {
                ObjectValue = new MegaNZNode {
                    Id = "aaa",
                    LastModificationDate = new DateTime(2016, 1, 1),
                    Name = "TestNode_[[2016-1-1-0-0-0]].jpeg",
                    Size = 100,
                    Type = CG.Web.MegaApiClient.NodeType.File
                }
            };

            var localNode = new LocalNode {
                Name = "TestNode.jpeg",
                LastModificationDate = megaNzNode.ObjectValue.LastModificationDate,
                FullPath = "c:\\some_path\\TestNode.jpeg",
                Size = megaNzNode.ObjectValue.Size,
                Type  = megaNzNode.ObjectValue.Type
            };

            var megaNzNodeHash = megaNzNode.GetHashCode();
            var localNodeHash = localNode.GetHashCode();

            Assert.AreEqual(megaNzNodeHash, localNodeHash);

        }
    }
}
