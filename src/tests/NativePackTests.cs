namespace OPack.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;
    using FluentAssertions;
    using TestStructs;

    public class NativePackTests
    {
        [Fact]
        public void PackSimpleStruct()
        {
            var nativePack = Packer.NativePack(new LongBoolShort());
            nativePack.Item1.Should().NotBeEmpty();
        }
    }
}