namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class UnpackTests
    {
        [Fact]
        public void UnpackLongValueBigEndian()
        {
            var returnValue = new Packer().Unpack(">q", default, new Packer().Pack(">q", 0, long.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(long.MaxValue);
        }

        [Fact]
        public void UnpackLongValueLittleEndian()
        {
            var returnValue = new Packer().Unpack("<q", default, new Packer().Pack("<q", 0, long.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(long.MaxValue);
        }

        [Fact]
        public void UnpackSignedByteBigEndian()
        {
            var returnValue = new Packer().Unpack(">b", default, new Packer().Pack(">b", 0, -128));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(-128);
        }

        [Fact]
        public void UnpackSignedByteLittleEndian()
        {
            var returnValue = new Packer().Unpack("<b", default, new Packer().Pack("<b", 0, -128));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(-128);
        }
    }
}