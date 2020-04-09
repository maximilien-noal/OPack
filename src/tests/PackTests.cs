using FluentAssertions;
using OPack;
using System;
using System.Globalization;
using Xunit;

namespace OPack.Tests
{
    public class PackTests
    {
        [Fact]
        public void CanPackInt32LittleEndian()
        {
            var returnValue = Packer.Pack("<b", 0, -128);
            returnValue.Should().HaveCount(1).And.BeEquivalentTo(128);
        }
    }
}