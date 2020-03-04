namespace OPack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Ref: https://stackoverflow.com/questions/28225303/equivalent-in-c-sharp-of-pythons-struct-pack-unpack
    /// This is a crude implementation of a format string based struct converter for C#. This is
    /// probably not the best implementation, the fastest implementation, the most bug-proof
    /// implementation, or even the most functional implementation. It's provided as-is for free. Enjoy.
    /// </summary>
    public static class Packer
    {
        // TODO : Make it accept the same parameters as Python's struct.pacck

        /// <summary>
        /// Convert an array of objects to a byte array, along with a string that can be used with Unpack.
        /// </summary>
        /// <param name="items"> An object array of items to convert. </param>
        /// <param name="isLittleEndian"> Set to False if you want to use big endian output. </param>
        /// <param name="neededFormatStringToRecover">
        /// Variable to place an 'Unpack'-compatible format string into.
        /// </param>
        /// <returns> A Byte array containing the objects provided in binary format. </returns>
        public static byte[] Pack(object[] items, bool isLittleEndian, out string neededFormatStringToRecover)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // make a byte list to hold the bytes of output
            List<byte> outputBytes = new List<byte>();

            // should we be flipping bits for proper endinanness?
            bool endianFlip = isLittleEndian != BitConverter.IsLittleEndian;

            // start working on the output string
            string outString = !isLittleEndian ? ">" : "<";

            // convert each item in the objects to the representative bytes
            foreach (object o in items)
            {
                byte[] theseBytes = TypeAgnosticGetBytes(o);
                if (endianFlip)
                {
                    theseBytes = (byte[])theseBytes.Reverse();
                }

                outString += GetFormatSpecifierFor(o);
                outputBytes.AddRange(theseBytes);
            }

            neededFormatStringToRecover = outString;

            return outputBytes.ToArray();
        }

        /// <summary> Packs and automatically finds the format to be used. </summary>
        /// <param name="items"> The struct to pack. </param>
        /// <returns> The packed objects. </returns>
        public static byte[] Pack(object[] items)
        {
            return Pack(items, true, out _);
        }

        /// <summary>
        /// Convert a byte array into an array of objects based on Python's "struct.unpack" protocol.
        /// </summary>
        /// <param name="format"> A "struct.pack"-compatible format string. </param>
        /// <param name="bytes"> An array of bytes to convert to objects. </param>
        /// <returns> Array of objects. </returns>
        /// <remarks>
        /// You are responsible for casting the objects in the array back to their proper types.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="format" /> doesn't correspond to the length of <paramref name="bytes" />.
        /// </exception>
        public static object[] Unpack(string format, byte[] bytes)
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

            // TODO : Manage endianess flip Little endian.
            if (format.Substring(0, 1) == "<")
            {
                format = format.Substring(1);
            }

            // Big endian.
            else if (format.Substring(0, 1) == ">")
            {
                format = format.Substring(1);
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
                    outputList.Add((object)(long)BitConverter.ToInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                    Debug.WriteLine("  Added signed 64-bit integer.");
                }
                else if (character == 'Q')
                {
                    outputList.Add((object)(ulong)BitConverter.ToUInt64(bytes, byteArrayPosition));
                    byteArrayPosition += 8;
                    Debug.WriteLine("  Added unsigned 64-bit integer.");
                }
                else if (character == 'l')
                {
                    outputList.Add((object)(int)BitConverter.ToInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                    Debug.WriteLine("  Added signed 32-bit integer.");
                }
                else if (character == 'L')
                {
                    outputList.Add((object)(uint)BitConverter.ToUInt32(bytes, byteArrayPosition));
                    byteArrayPosition += 4;
                    Debug.WriteLine("  Added unsignedsigned 32-bit integer.");
                }
                else if (character == 'h')
                {
                    outputList.Add((object)(short)BitConverter.ToInt16(bytes, byteArrayPosition));
                    byteArrayPosition += 2;
                    Debug.WriteLine("  Added signed 16-bit integer.");
                }
                else if (character == 'H')
                {
                    outputList.Add((object)(ushort)BitConverter.ToUInt16(bytes, byteArrayPosition));
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
                    outputList.Add((object)(byte)buf[0]);
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
                    throw new ArgumentException("You should not be here.");
                }
            }

            return outputList.ToArray();
        }

        private static string GetFormatSpecifierFor(object o)
        {
            if (o is int)
            {
                return "i";
            }

            if (o is uint)
            {
                return "I";
            }

            if (o is long)
            {
                return "q";
            }

            if (o is ulong)
            {
                return "Q";
            }

            if (o is short)
            {
                return "h";
            }

            if (o is ushort)
            {
                return "H";
            }

            if (o is byte)
            {
                return "B";
            }

            if (o is sbyte)
            {
                return "b";
            }

            throw new ArgumentException("Unsupported object type found");
        }

        /// <summary>
        /// We use this function to provide an easier way to type-agnostically call the GetBytes
        /// method of the BitConverter class. This means we can have much cleaner code above.
        /// </summary>
        private static byte[] TypeAgnosticGetBytes(object o)
        {
            if (o is int x)
            {
                return BitConverter.GetBytes(x);
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