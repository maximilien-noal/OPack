namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class PackTests
    {
        [Fact]
        public void PackSignedByteBigEndian()
        {
            var returnValue = Packer.Pack(">b", 0, -128);
            returnValue.Should().HaveCount(1).And.BeEquivalentTo(128);
        }

        [Fact]
        public void PackSignedByteLittleEndian()
        {
            var returnValue = Packer.Pack("<b", 0, -128);
            returnValue.Should().HaveCount(1).And.BeEquivalentTo(128);
        }
    }
}