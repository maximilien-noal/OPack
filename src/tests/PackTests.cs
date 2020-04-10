namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class PackTests
    {
        [Fact]
        public void PackLongValueBigEndian()
        {
            var returnValue = Packer.Pack(">q", 0, long.MaxValue);
            returnValue.Should().HaveCount(8).And.StartWith(0x7F).And.OnlyContain(x => (x == 0X7F && x == returnValue[0]) || x == 0xFF);
        }

        [Fact]
        public void PackLongValueLittleEndian()
        {
            var returnValue = Packer.Pack("<q", 0, long.MaxValue);
            returnValue.Should().HaveCount(8).And.EndWith(0x7F).And.OnlyContain(x => (x == 0X7F && x == returnValue[7]) || x == 0xFF);
        }

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