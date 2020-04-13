namespace OPack.Tests
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    public class AutoPackTests
    {
        [Fact]
        public void AutoPackLong()
        {
            var returnValue = Packer.AutoPack(default, long.MaxValue);
            returnValue.Item1.Should().HaveCount(8);
            returnValue.Item2.Should().Be("q");
        }
    }
}