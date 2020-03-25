namespace OPack
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary> A translation of Python's pack and unpack protocol to C#. </summary>
    public static class Packer
    {
        private static char[] prefixes = { '<', '>', '@', '=', '!' };

        /// <summary>
        /// Convert an array of objects to a byte array, along with a string that can be used with Unpack.
        /// </summary>
        /// <param name="format"> The format to use for packing. </param>
        /// <param name="items"> An object array of items to convert. </param>
        /// <returns> A Byte array containing the objects provided in binary format. </returns>
        public static byte[] Pack(string format, params object[] items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentNullException(nameof(format));
            }

            bool isBigEndian = AreWeInBigEndianMode(format, items);

            List<byte> outputBytes = new List<byte>();

            // convert each item in the objects to the representative bytes
            foreach (object o in items)
            {
                byte[] theseBytes = TypeAgnosticGetBytes(o, isBigEndian);
                outputBytes.AddRange(theseBytes);
            }

            return outputBytes.ToArray();
        }

        /// <summary>
        /// Convert a byte array into an array of numerical value types based on Python's
        /// "struct.unpack" protocol.
        /// </summary>
        /// <param name="format"> A "struct.unpack"-compatible format string. </param>
        /// <param name="bytes"> An array of bytes to convert to objects. </param>
        /// <returns> Array of objects. </returns>
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

            // First we parse the format string to make sure it's proper.
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("Format string cannot be empty.");
            }

            // Now, we find out how long the byte array needs to be
            int totalByteLength = 0;
            foreach (char c in format)
            {
                switch (c)
                {
                    case 'q':
                    case 'Q':
                        totalByteLength += 8;
                        break;

                    case 'i':
                    case 'I':
                        totalByteLength += 4;
                        break;

                    case 'h':
                    case 'H':
                        totalByteLength += 2;
                        break;

                    case 'b':
                    case 'B':
                    case 'x':
                        totalByteLength++;
                        break;

                    default:
                        throw new ArgumentException("Invalid character found in format string.");
                }
            }

            // Test the byte array length to see if it contains as many bytes as is needed for the string.
            if (bytes.Length != totalByteLength)
            {
                throw new ArgumentException("The number of bytes provided does not match the total length of the format string.");
            }

            // Ok, we can go ahead and start parsing bytes!
            int byteArrayPosition = 0;
            List<object> outputList = new List<object>();
            byte[] buf;
            foreach (char character in format)
            {
                if (character == 'q')
                {
                    outputList.Add((object)BitConverter.ToInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                    Debug.WriteLine("  Added signed 64-bit integer.");
                }
                else if (character == 'Q')
                {
                    outputList.Add((object)BitConverter.ToUInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                    Debug.WriteLine("  Added unsigned 64-bit integer.");
                }
                else if (character == 'l')
                {
                    outputList.Add((object)BitConverter.ToInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                    Debug.WriteLine("  Added signed 32-bit integer.");
                }
                else if (character == 'L')
                {
                    outputList.Add((object)BitConverter.ToUInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                    Debug.WriteLine("  Added unsignedsigned 32-bit integer.");
                }
                else if (character == 'h')
                {
                    outputList.Add((object)BitConverter.ToInt16(bytes, byteArrayPosition));
                    byteArrayPosition += 2;
                    Debug.WriteLine("  Added signed 16-bit integer.");
                }
                else if (character == 'H')
                {
                    outputList.Add((object)BitConverter.ToUInt16(bytes, byteArrayPosition));
                    byteArrayPosition += 2;
                    Debug.WriteLine("  Added unsigned 16-bit integer.");
                }
                else if (character == 'b')
                {
                    buf = new byte[1];
                    Array.Copy(bytes, byteArrayPosition, buf, 0, 1);
                    outputList.Add((object)(sbyte)buf[0]);
                    byteArrayPosition++;
                    Debug.WriteLine("  Added signed byte");
                }
                else if (character == 'B')
                {
                    buf = new byte[1];
                    Array.Copy(bytes, byteArrayPosition, buf, 0, 1);
                    outputList.Add((object)buf[0]);
                    byteArrayPosition++;
                    Debug.WriteLine("  Added unsigned byte");
                }
                else if (character == 'x')
                {
                    byteArrayPosition++;
                    Debug.WriteLine("  Ignoring a byte");
                }
                else
                {
                    throw new InvalidOperationException($"Invalid character found in arg {nameof(format)}.");
                }
            }

            return outputList.ToArray();
        }

        private static bool AreWeInBigEndianMode(string format, object[] items)
        {
            var selectedPrefix = '@';

            int formatLength = format.Length;

            if (!prefixes.Contains(format[0]))
            {
                formatLength--;
            }
            else
            {
                selectedPrefix = format[0];
            }

            if (formatLength < items.Length)
            {
                throw new InvalidOperationException("The number of items must match the format length minus the endianness character");
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
        /// <param name="o"> The numerical value type to pack into an array of bytes. </param>
        /// <param name="isBigEndian"> Do we use little or big endian mode. </param>
        private static byte[] TypeAgnosticGetBytes(object o, bool isBigEndian)
        {
            if (o is int x)
            {
                Span<byte> holder = stackalloc byte[4];
                if (isBigEndian)
                {
                    BinaryPrimitives.WriteInt32BigEndian(holder, (int)o);
                }
                else
                {
                    BinaryPrimitives.WriteInt32LittleEndian(holder, (int)o);
                }

                return holder.ToArray();
            }

            if (o is uint x2)
            {
                return BitConverter.GetBytes(x2);
            }

            if (o is long x3)
            {
                return BitConverter.GetBytes(x3);
            }

            if (o is ulong x4)
            {
                return BitConverter.GetBytes(x4);
            }

            if (o is short x5)
            {
                return BitConverter.GetBytes(x5);
            }

            if (o is ushort x6)
            {
                return BitConverter.GetBytes(x6);
            }

            if (o is byte || o is sbyte)
            {
                return new byte[] { (byte)o };
            }

            throw new ArgumentException("Unsupported object type found");
        }
    }
}