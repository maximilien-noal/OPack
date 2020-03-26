namespace OPack
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A translation of Python's pack and unpack protocol to C#.
    /// </summary>
    public static class Packer
    {
        private static char[] endiannessPrefixes = { '<', '>', '@', '=', '!' };

        /// <summary>
        /// Convert an array of objects to a little endian byte array, and a string that can be used
        /// with <see cref="Unpack(string, byte[])" />.
        /// </summary>
        /// <param name="items">An object array of value types to convert.</param>
        /// <returns>
        /// A <see cref="Tuple{T1, T2}" /> Byte array containing the objects provided in binary format.
        /// </returns>
        public static Tuple<byte[], string> AutoPackLittleEndian(params object[] items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<byte> outputBytes = new List<byte>();

            foreach (object o in items)
            {
                byte[] theseBytes = TypeAgnosticGetBytes(o, false);
                outputBytes.AddRange(theseBytes);
            }

            return Tuple.Create(outputBytes.ToArray(), string.Empty);
        }

        /// <summary>
        /// Return the size of the struct (and hence of the bytes object produced by
        /// <see cref="AutoPackLittleEndian(object[])" /> corresponding to the format string format.
        /// </summary>
        /// <param name="format">The format to be used for packing.</param>
        /// <returns>The size of the struct.</returns>
        public static int CalcSize(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentNullException($"{nameof(format)} cannot be null or empty.");
            }

            string formatWithoutEndianness = format;

            if (endiannessPrefixes.Contains(format[0]))
            {
                formatWithoutEndianness = format.Substring(1);
            }

            int totalByteLength = 0;
            foreach (char c in formatWithoutEndianness)
            {
                switch (c)
                {
                    case 'q':
                    case 'Q':
                    case 'd':
                        totalByteLength += 8;
                        break;

                    case 'i':
                    case 'I':
                    case 'l':
                    case 'L':
                    case 'f':
                        totalByteLength += 4;
                        break;

                    case 'h':
                    case 'H':
                        totalByteLength += 2;
                        break;

                    case 'b':
                    case '?':
                    case 'B':
                    case 'x':
                        totalByteLength++;
                        break;

                    default:
                        throw new ArgumentException($"Invalid character found in format string : {c}.");
                }
            }

            return totalByteLength;
        }

        /// <summary>
        /// Convert a byte array into an array of numerical value types based on Python's
        /// "struct.unpack" protocol.
        /// </summary>
        /// <param name="format">A "struct.unpack"-compatible format string.</param>
        /// <param name="bytes">An array of bytes to convert to objects.</param>
        /// <returns>Array of objects.</returns>
        /// <remarks>
        /// You are responsible for casting the objects in the array back to their proper types.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="format" /> doesn't correspond to the length of <paramref name="bytes" />.
        /// </exception>
        public static object[] Unpack(string format, params byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (bytes.Length != CalcSize(format))
            {
                throw new ArgumentException("The number of bytes provided does not match the total length of the format string.");
            }

            //bool useBigEndian = AreWeInBigEndianMode(format);
            int byteArrayPosition = 0;
            List<object> outputList = new List<object>();
            foreach (char character in format)
            {
                if (character == 'q')
                {
                    outputList.Add(BitConverter.ToInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                }
                else if (character == 'Q')
                {
                    outputList.Add(BitConverter.ToUInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                }
                else if (character == 'l')
                {
                    outputList.Add(BitConverter.ToInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                }
                else if (character == 'L')
                {
                    outputList.Add(BitConverter.ToUInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                }
                else if (character == 'h')
                {
                    outputList.Add(BitConverter.ToInt16(bytes, byteArrayPosition));
                    byteArrayPosition += 2;
                }
                else if (character == 'H')
                {
                    outputList.Add(BitConverter.ToUInt16(bytes, byteArrayPosition));
                    byteArrayPosition += 2;
                }
                else if (character == 'b' || character == 'B')
                {
                    byte[] buf = new byte[1];
                    Array.Copy(bytes, byteArrayPosition, buf, 0, 1);
                    byte value = buf[0];
                    if (character == 'b')
                    {
                        outputList.Add((sbyte)value);
                    }
                    else
                    {
                        outputList.Add(value);
                    }

                    byteArrayPosition++;
                }
                else if (character == 'x')
                {
                    byteArrayPosition++;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid character '{character}' found in arg {nameof(format)}.");
                }
            }

            return outputList.ToArray();
        }

        private static bool AreWeInBigEndianMode(string format, object[] items)
        {
            var selectedPrefix = '@';

            int formatLength = format.Length;

            if (!endiannessPrefixes.Contains(format[0]))
            {
                formatLength--;
            }
            else
            {
                selectedPrefix = format[0];
            }

            if (formatLength < items.Length)
            {
                throw new InvalidOperationException("The number of items must match the format length minus the start endianness character (if present).");
            }

            bool isBigEndian = false;

            if (selectedPrefix == '@' || selectedPrefix == '=')
            {
                isBigEndian = !BitConverter.IsLittleEndian;
            }

            if (selectedPrefix == '<')
            {
                isBigEndian = false;
            }

            if (selectedPrefix == '>' || selectedPrefix == '!')
            {
                isBigEndian = true;
            }

            return isBigEndian;
        }

        /// <summary>
        /// We use this function to provide an easier way to type-agnostically call the GetBytes
        /// method of the BitConverter class. This means we can have much cleaner code above.
        /// </summary>
        /// <param name="boxedValue">The numerical value type to pack into an array of bytes.</param>
        /// <param name="isBigEndian">Do we use little or big endian mode.</param>
        private static byte[] TypeAgnosticGetBytes(object boxedValue, bool isBigEndian)
        {
            if (boxedValue is int signedInteger)
            {
                Span<byte> holder = stackalloc byte[4];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteInt32BigEndian(holder, signedInteger);
                }
                else
                {
                    BinaryPrimitives.WriteInt32LittleEndian(holder, signedInteger);
                }

                return holder.ToArray();
            }
            else if (boxedValue is uint unsignedInteger)
            {
                Span<byte> holder = stackalloc byte[4];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteUInt32BigEndian(holder, unsignedInteger);
                }
                else
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(holder, unsignedInteger);
                }
            }
            else if (boxedValue is long signedLongInteger)
            {
                Span<byte> holder = stackalloc byte[8];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteInt64BigEndian(holder, signedLongInteger);
                }
                else
                {
                    BinaryPrimitives.WriteInt64LittleEndian(holder, signedLongInteger);
                }
            }
            else if (boxedValue is ulong unsignedLongInteger)
            {
                Span<byte> holder = stackalloc byte[8];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(holder, unsignedLongInteger);
                }
                else
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(holder, unsignedLongInteger);
                }
            }
            else if (boxedValue is short signedShort)
            {
                Span<byte> holder = stackalloc byte[2];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteInt16BigEndian(holder, signedShort);
                }
                else
                {
                    BinaryPrimitives.WriteInt16LittleEndian(holder, signedShort);
                }
            }
            else if (boxedValue is ushort unsignedShort)
            {
                Span<byte> holder = stackalloc byte[2];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteUInt16BigEndian(holder, unsignedShort);
                }
                else
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(holder, unsignedShort);
                }
            }
            else if (boxedValue is byte || boxedValue is sbyte)
            {
                return new byte[] { (byte)boxedValue };
            }

            throw new ArgumentException("Unsupported object type found. We can pack only numerical value types.");
        }
    }
}