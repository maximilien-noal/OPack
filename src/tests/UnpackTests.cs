namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class UnpackTests
    {
        [Fact]
        public void UnpackBoolValue()
        {
            var returnValue = new Packer().Unpack("=?", default, new Packer().Pack("=?", 0, 1));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(true);
        }

        [Fact]
        public void UnpackBoolValueAtSecondIndex()
        {
            var returnValue = new Packer().Unpack("=??", 1, new Packer().Pack("=??", 0, 1, 0));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(false);
        }

        [Fact]
        public void UnpackDoubleValueBigEndian()
        {
            var returnValue = new Packer().Unpack(">d", default, new Packer().Pack(">d", default, double.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(double.MaxValue);
        }

        [Fact]
        public void UnpackDoubleValueLittleEndian()
        {
            var returnValue = new Packer().Unpack("<d", default, new Packer().Pack("<d", default, double.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(double.MaxValue);
        }

        [Fact]
        public void UnpackLongValueBigEndian()
        {
            var returnValue = new Packer().Unpack(">q", default, new Packer().Pack(">q", default, long.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(long.MaxValue);
        }

        [Fact]
        public void UnpackLongValueLittleEndian()
        {
            var returnValue = new Packer().Unpack("<q", default, new Packer().Pack("<q", default, long.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(long.MaxValue);
        }

        [Fact]
        public void UnpackShortValueBigEndian()
        {
            var returnValue = new Packer().Unpack(">h", default, new Packer().Pack(">h", default, short.MaxValue));
            returnValue.Should().Equal(short.MaxValue);
        }

        [Fact]
        public void UnpackShortValueLittleEndian()
        {
            var returnValue = new Packer().Unpack("<h", default, new Packer().Pack("<h", default, short.MaxValue));
            returnValue.Should().Equal(short.MaxValue);
        }

        [Fact]
        public void UnpackSignedByteBigEndian()
        {
            var returnValue = new Packer().Unpack(">b", default, new Packer().Pack(">b", default, -128));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(-128);
        }

        [Fact]
        public void UnpackSignedByteLittleEndian()
        {
            var returnValue = new Packer().Unpack("<b", default, new Packer().Pack("<b", default, -128));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(-128);
        }

        [Fact]
        public void UnpackSingleValueBigEndian()
        {
            var returnValue = new Packer().Unpack(">f", default, new Packer().Pack(">f", default, float.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(float.MaxValue);
        }

        [Fact]
        public void UnpackSingleValueLittleEndian()
        {
            var returnValue = new Packer().Unpack("<f", default, new Packer().Pack("<f", default, float.MaxValue));
            returnValue.Should().ContainSingle().And.BeEquivalentTo(float.MaxValue);
        }
    }
}