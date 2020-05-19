namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class StandardSizesTests
    {
        [Fact]
        public void BitAndUnsignedIntAreOnThreeBits()
        {
            var returnValue = new Packer().Pack("<BBH", default, 202, 0, 1);
            returnValue.Should().ContainInOrder(202, 0, 1, 0).And.HaveCount(4);
        }

        [Fact]
        public void UnsignedIntIsOnTwoBits()
        {
            var returnValue = new Packer().Pack("<H", default, 1);
            returnValue.Should().ContainInOrder(1, 0).And.HaveCount(2);
        }
    }
}