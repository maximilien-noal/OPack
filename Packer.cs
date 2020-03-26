namespace OPack
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary> A translation of Python's pack and unpack protocol to C#. </summary>
    public static class Packer
    {
        private static char[] endiannessPrefixes = { '<', '>', '@', '=', '!' };

        /// <summary>
        /// Return the size of the struct (and hence of the bytes object produced by
        /// <see cref="Pack(int, object[])" /> corresponding to the format string format.
        /// </summary>
        /// <param name="format"> The format to be used for packing. </param>
        /// <returns> The size of the struct. </returns>
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
        /// Convert an array of objects to a little endian byte array, while following the specified format.
        /// </summary>
        /// <param name="format"> A "struct.unpack"-compatible format string. </param>
        /// <param name="offset"> Where to start packing in the provided <paramref name="items" />. </param>
        /// <param name="items"> An array of items to convert to a byte array. </param>
        /// <returns> A byte array of packed elements. </returns>
        public static byte[] Pack(string format, int offset = 0, params object[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentNullException(nameof(format));
            }

            int formatLength = format.Length;
            int formatOffset = 0;

            if (endiannessPrefixes.Contains(format[0]))
            {
                formatLength--;
                formatOffset = 1;
            }

            if (items.Length != formatLength)
            {
                throw new ArgumentException($"The number of {nameof(items)} provided does not match the total length of the ${nameof(format)} string.");
            }

            bool useBigEndian = AreWeInBigEndianMode(format);

            List<byte> outputBytes = new List<byte>();

            for (int i = formatOffset; i < format.Length; i++)
            {
                var item = items[i + offset];
                var character = format[i];

                if (character == '?' || character == 'B')
                {
                    byte convertedItem = Convert.ToByte(item, CultureInfo.InvariantCulture);
                    outputBytes.Add(convertedItem);
                }
                else if (character == 'b')
                {
                    sbyte convertedItem = Convert.ToSByte(item, CultureInfo.InvariantCulture);
                    outputBytes.Add((byte)convertedItem);
                }
                else if (character == 'i')
                {
                    Span<byte> dest = stackalloc byte[4];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteInt32BigEndian(dest, (int)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt32LittleEndian(dest, (int)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else if (character == 'I')
                {
                    Span<byte> dest = stackalloc byte[4];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(dest, (uint)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(dest, (uint)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else if (character == 'q' || character == 'l')
                {
                    Span<byte> dest = stackalloc byte[8];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteInt64BigEndian(dest, (int)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt64LittleEndian(dest, (int)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else if (character == 'Q' || character == 'L')
                {
                    Span<byte> dest = stackalloc byte[8];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteUInt64BigEndian(dest, (uint)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt64LittleEndian(dest, (uint)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else if (character == 'h')
                {
                    Span<byte> dest = stackalloc byte[2];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteInt16BigEndian(dest, (short)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(dest, (short)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else if (character == 'H')
                {
                    Span<byte> dest = stackalloc byte[2];
                    if (useBigEndian)
                    {
                        BinaryPrimitives.WriteUInt16BigEndian(dest, (ushort)item);
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(dest, (ushort)item);
                    }

                    outputBytes.AddRange(dest.ToArray());
                }
                else
                {
                    throw new InvalidOperationException($"Invalid character '{character}' found in arg {nameof(format)}.");
                }
            }

            return outputBytes.ToArray();
        }

        /// <summary>
        /// Convert an array of objects to a little endian byte array, and a string that can be used
        /// with <see cref="Unpack(string, int, byte[])" />.
        /// </summary>
        /// <param name="offset"> Where to start packing in the provided <paramref name="items" />. </param>
        /// <param name="items"> An object array of value types to convert. </param>
        /// <returns>
        /// A <see cref="Tuple{T1, T2}" /> Byte array containing the objects provided in binary format.
        /// </returns>
        public static Tuple<byte[], string> Pack(int offset = 0, params object[] items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            StringBuilder format = new StringBuilder();

            List<byte> outputBytes = new List<byte>();

            for (int i = offset; i < items.Length; i++)
            {
                var obj = items[i];
                byte[] theseBytes = TypeAgnosticGetBytes(obj, false);
                format.Append(GetFormatSpecifierFor(obj));
                outputBytes.AddRange(theseBytes);
            }

            return Tuple.Create(outputBytes.ToArray(), format.ToString());
        }

        /// <summary>
        /// Convert a byte array into an array of numerical value types based on Python's
        /// "struct.unpack" protocol.
        /// </summary>
        /// <param name="format"> A "struct.unpack"-compatible format string. </param>
        /// <param name="offset"> Where to start unpacking in the provided <paramref name="bytes" />. </param>
        /// <param name="bytes"> An array of bytes to convert to objects. </param>
        /// <returns> Array of objects. </returns>
        /// <remarks>
        /// You are responsible for casting the objects in the array back to their proper types.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="format" /> doesn't correspond to the length of <paramref name="bytes" />.
        /// </exception>
        public static object[] Unpack(string format, int offset = 0, params byte[] bytes)
        {
            if (bytes is null)
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

            bool useBigEndian = AreWeInBigEndianMode(format);

            int byteArrayPosition = offset;
            List<object> outputList = new List<object>();
            foreach (char character in format)
            {
                if (character == 'q')
                {
                    var array = new byte[8];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadInt64BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadInt64LittleEndian(array));
                    }

                    byteArrayPosition += 8;
                }
                else if (character == 'Q')
                {
                    var array = new byte[8];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt64BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt64LittleEndian(array));
                    }

                    byteArrayPosition += 8;
                }
                else if (character == 'l')
                {
                    var array = new byte[4];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadInt32BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadInt32LittleEndian(array));
                    }

                    byteArrayPosition += 4;
                }
                else if (character == 'L')
                {
                    var array = new byte[4];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt32BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt32LittleEndian(array));
                    }

                    byteArrayPosition += 4;
                }
                else if (character == 'h')
                {
                    var array = new byte[2];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadInt16BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadInt16LittleEndian(array));
                    }

                    byteArrayPosition += 2;
                }
                else if (character == 'H')
                {
                    var array = new byte[2];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    if (useBigEndian)
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt16BigEndian(array));
                    }
                    else
                    {
                        outputList.Add(BinaryPrimitives.ReadUInt16LittleEndian(array));
                    }

                    byteArrayPosition += 2;
                }
                else if (character == 'b' || character == 'B')
                {
                    byte[] array = new byte[1];
                    Array.Copy(bytes, byteArrayPosition, array, 0, array.Length);
                    byte value = array[0];
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

        private static bool AreWeInBigEndianMode(string format)
        {
            var selectedPrefix = '@';

            if (endiannessPrefixes.Contains(format[0]))
            {
                selectedPrefix = format[0];
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

        private static string GetFormatSpecifierFor(object obj)
        {
            if (obj is bool)
            {
                return "?";
            }

            if (obj is double)
            {
                return "d";
            }

            if (obj is float)
            {
                return "f";
            }

            if (obj is int)
            {
                return "i";
            }

            if (obj is uint)
            {
                return "I";
            }

            if (obj is long)
            {
                return "q";
            }

            if (obj is ulong)
            {
                return "Q";
            }

            if (obj is short)
            {
                return "h";
            }

            if (obj is ushort)
            {
                return "H";
            }

            if (obj is byte)
            {
                return "B";
            }

            if (obj is sbyte)
            {
                return "b";
            }

            throw new ArgumentException("Unsupported object type found");
        }

        /// <summary>
        /// We use this function to provide an easier way to type-agnostically call the GetBytes
        /// method of the BitConverter class. This means we can have much cleaner code above.
        /// </summary>
        /// <param name="boxedValue"> The numerical value type to pack into an array of bytes. </param>
        /// <param name="isBigEndian"> Do we use little or big endian mode. </param>
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