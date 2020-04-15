namespace OPack.Tests
{
    using FluentAssertions;
    using FluentAssertions.Common;
    using TestStructs;
    using OPack;

    using Xunit;

    public class NativeMethodsTests
    {
        [Fact]
        public void CalcSizeNative()
        {
            var nativeSize = Packer.NativeCalcSize<LongBoolShort>();
            nativeSize.Should().IsSameOrEqualTo(24);
        }

        [Fact]
        public void CalcSizeNativeExt()
        {
            var nativeSize = new LongBoolShort().CalcSize();
            nativeSize.Should().IsSameOrEqualTo(Packer.NativeCalcSize<LongBoolShort>());
        }

        [Fact]
        public void UnpackSimpleStruct()
        {
            var packedAndBroughtBack = Packer.NativeUnpack<LongBoolShort>(Packer.NativePack(new LongBoolShort()));
            packedAndBroughtBack.Should().BeEquivalentTo(new LongBoolShort());
        }

        [Fact]
        public void UnpackSimpleStructExt()
        {
            var packedAndBroughtBack = new LongBoolShort().Unpack(new LongBoolShort().Pack());
            packedAndBroughtBack.Should().BeEquivalentTo(Packer.NativeUnpack<LongBoolShort>(Packer.NativePack(new LongBoolShort())));
        }
    }
}