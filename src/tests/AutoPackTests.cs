namespace OPack.Tests
{
    using FluentAssertions;

    using Xunit;

    public class AutoPackTests
    {
        [Fact]
        public void AutoPackLong()
        {
            var returnValue = Packer.AutoPack(default, long.MaxValue);
            returnValue.Item1.Should().ContainInOrder(255, 255, 255, 255, 255, 255, 255, 127).And.HaveCount(8);
            returnValue.Item2.Should().Be("q");
        }

        [Fact]
        public void CipUnconnectedSendPackTest()
        {
            byte cipService = 0x52;
            byte cipPathSize = 0x02;
            byte cipClassType = 0x20;
            byte cipClass = 0x06;
            byte cipInstanceType = 0x24;
            byte cipInstance = 0x01;
            byte cipPriority = 0x0A;
            byte cipTimeoutTicks = 0x0e;
            uint ServiceSize = 0x06;

            //format : "<BBBBBBBBH"
            var packedValue = Packer.AutoPack(default, cipService, cipPathSize, cipClassType, cipClass, cipInstanceType, cipInstance, cipPriority, cipTimeoutTicks, ServiceSize);
            packedValue.Item1.Should().ContainInOrder(82, 32, 6, 36, 1, 10, 14, 0, 0, 0).And.HaveCount(12);
        }

        [Fact]
        public void EIPHeaderPackTest()
        {
            var ioiLength = 16;
            byte eipConnectedDataLength = (byte)(ioiLength + 2);
            ushort eipCommand = 0x70;
            ushort eipLength = (ushort)(22 + ioiLength);
            uint eipSessionHandle = 541240;
            uint eipStatus = 0x00;
            ulong eipContext = 0x848198494;
            uint eipOptions = 0x0000;
            uint eipInterfaceHandle = 0x00;
            ushort eipTimeout = 0x00;
            ushort eipItemCount = 0x02;
            ushort eipItem1ID = 0xA1;
            ushort eipItem1Length = 0x04;
            uint eipItem1 = 64515;
            ushort eipItem2ID = 0xB1;
            ushort eipItem2Length = eipConnectedDataLength;
            ushort sequenceCounter = 125;
            ushort eipSequence = sequenceCounter;

            //format: "<HHIIQIIHHHHIHHH"
            var eipHeaderFrame = Packer.AutoPack(default,
                eipCommand,
                eipLength,
                eipSessionHandle,
                eipStatus,
                eipContext,
                eipOptions,
                eipInterfaceHandle,
                eipTimeout,
                eipItemCount,
                eipItem1ID,
                eipItem1Length,
                eipItem1,
                eipItem2ID,
                eipItem2Length,
                eipSequence);

            eipHeaderFrame.Item1.Should().ContainInOrder(112, 0, 38, 0, 56, 66, 8, 0, 0, 0, 0, 0, 148, 132, 25, 72, 8, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 161, 0, 4, 0, 3, 252, 0, 0, 177, 0, 18, 125, 0).And.HaveCount(46);
        }

        [Fact]
        public void EipRRDataHeaderPackTest()
        {
            ushort eipCommand = 0x6F;
            ushort eipLength = (ushort)(16 + 2);
            uint eipSessionHandle = uint.MaxValue;
            uint eipStatus = 0x00;
            ulong eipContext = ulong.MaxValue;
            uint eipOptions = 0x00;
            uint eipInterfaceHandle = 0x00;
            ushort eipTimeout = 0x00;
            ushort eipItemCount = 0x02;
            ushort eipItem1Type = 0x00;
            ushort eipItem1Length = 0x00;
            ushort eipItem2Type = 0xB2;
            ushort eipItem2Length = (ushort)2;

            //format: "<HHIIQIIHHHHHH"
            var packedValue = Packer.AutoPack(default,
                eipCommand,
                eipLength,
                eipSessionHandle,
                eipStatus,
                eipContext,
                eipOptions,
                eipInterfaceHandle,
                eipTimeout,
                eipItemCount,
                eipItem1Type,
                eipItem1Length,
                eipItem2Type,
                eipItem2Length);

            packedValue.Item1.Should().ContainInOrder(
                111, 0, 18, 0, 255, 255, 255, 255, 0, 0,
                0, 0, 255, 255, 255, 255, 255, 255, 255, 255,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                2, 0, 0, 0, 0, 0, 178, 0, 2, 0).And.HaveCount(40);
        }

        [Fact]
        public void ListIdentityPackTest()
        {
            ushort listService = 0x63;
            ushort listLength = 0x00;
            uint listSessionHandle = 0x00;

            uint listStatus = 0x00;
            ushort listResponse = 0xFA;
            ushort listContext1 = 0x6948;
            ushort listContext2 = 0x6f4d;
            ushort listContext3 = 0x006d;
            uint listOptions = 0x00;

            //format: "<HHIIHHHHI"
            var packedValue = Packer.AutoPack(default, listService, listLength, listSessionHandle, listStatus, listResponse, listContext1, listContext2, listContext3, listOptions);
            packedValue.Item1.Should().ContainInOrder(
                99, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 250, 0, 72, 105, 77, 111, 109, 0,
                0, 0, 0, 0).And.HaveCount(24);
        }

        [Fact]
        public void MultiServiceHeaderPackTest()
        {
            byte multiService = 0X0A;
            byte multiPathSize = 0x02;
            byte MutliClassType = 0x20;
            byte multiClassSegment = 0x02;
            byte multiInstanceType = 0x24;
            byte multiInstanceSegment = 0x01;

            //format: "<BBBBBB"
            var packedValue = Packer.AutoPack(default, multiService, multiPathSize, MutliClassType, multiClassSegment, multiInstanceType, multiInstanceSegment);
            packedValue.Item1.Should().ContainInOrder(10, 2, 32, 2, 36, 1).And.HaveCount(6);
        }

        [Fact]
        public void ReadServicePackTest()
        {
            byte requestService = 0x52;
            byte requestPathSize = (byte)(16 / 2);

            //format: "<BB"
            Packer.AutoPack(default, requestService, requestPathSize).Item1.Should().ContainInOrder(0x52, 0x8).And.HaveCount(2);
        }
    }
}