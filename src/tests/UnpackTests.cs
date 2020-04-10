namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class UnpackTests
    {
        [Fact]
        public void UnpackSignedByteBigEndian()
        {
            var returnValue = Packer.Unpack(">b", default, Packer.Pack(">b", 0, -128));
            returnValue.Should().HaveCount(1).And.BeEquivalentTo(-128);
        }

        [Fact]
        public void UnpackSignedByteLittleEndian()
        {
            var returnValue = Packer.Unpack("<b", default, Packer.Pack("<b", 0, -128));
            returnValue.Should().HaveCount(1).And.BeEquivalentTo(-128);
        }
    }
}