namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class PackTests
    {
        [Fact]
        public void PackBoolValueAtSecondIndex()
        {
            var returnValue = new Packer().Pack("=??", 1, 1, 0);
            returnValue.Should().ContainSingle().And.BeEquivalentTo(0);
        }

        [Fact]
        public void PackLongValueBigEndian()
        {
            var returnValue = new Packer().Pack(">q", 0, long.MaxValue);
            returnValue.Should().HaveCount(8).And.StartWith(0x7F).And.OnlyContain(x => (x == 0X7F && x == returnValue[0]) || x == 0xFF);
        }

        [Fact]
        public void PackLongValueLittleEndian()
        {
            var returnValue = new Packer().Pack("<q", 0, long.MaxValue);
            returnValue.Should().HaveCount(8).And.EndWith(0x7F).And.OnlyContain(x => (x == 0X7F && x == returnValue[7]) || x == 0xFF);
        }

        [Fact]
        public void PackShortValueBigEndian()
        {
            var returnValue = new Packer().Pack(">h", 0, short.MaxValue);
            returnValue.Should().HaveCount(2).And.StartWith(0x7F).And.EndWith(0xFF);
        }

        [Fact]
        public void PackShortValueLittleEndian()
        {
            var returnValue = new Packer().Pack("<h", 0, short.MaxValue);
            returnValue.Should().HaveCount(2).And.StartWith(0xFF).And.EndWith(0x7F);
        }

        [Fact]
        public void PackSignedByteBigEndian()
        {
            var returnValue = new Packer().Pack(">b", 0, -128);
            returnValue.Should().ContainSingle().And.BeEquivalentTo(128);
        }

        [Fact]
        public void PackSignedByteLittleEndian()
        {
            var returnValue = new Packer().Pack("<b", 0, -128);
            returnValue.Should().ContainSingle().And.BeEquivalentTo(128);
        }
    }
}