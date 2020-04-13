namespace OPack.Tests
{
    using FluentAssertions;

    using System.Runtime.InteropServices;
    using TestStructs;
    using Xunit;

    public class CalcSizeTests
    {
        private const string COMPLEX_STRUCT = "BHhIIe?qdefbc";

        [Fact]
        public void CalcComplexStructSizeBigEndian()
        {
            var returnValue = new Packer().CalcSize($">{COMPLEX_STRUCT}");
            returnValue.Should().Be(40);
        }

        [Fact]
        public void CalcComplexStructSizeLittleEndian()
        {
            var returnValue = new Packer().CalcSize($"<{COMPLEX_STRUCT}");
            returnValue.Should().Be(40);
        }

        [Fact]
        public void CalcNativeStructSize()
        {
            var size = Marshal.SizeOf(new LongBoolShort());
            var returnValue = Packer.NativeCalcSize(new LongBoolShort());
            returnValue.Should().Be(size);
        }
    }
}