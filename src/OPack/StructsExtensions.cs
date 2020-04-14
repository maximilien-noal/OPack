namespace OPack
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary> The Packer API for native structs, as a set of extension methods to <see langword="struct" />. </summary>
    public static class StructsExtensions
    {
        /// <summary> Calculates the size of the <see langword="struct" /> in unmanaged memory. </summary>
        /// <typeparam name="T"> typeof(struct). </typeparam>
        /// <param name="target"> The struct to give to <see cref="Marshal.SizeOf{T}(T)" />. </param>
        /// <returns> The native size of the struct. </returns>
        public static int NativeCalcSize<T>(this T target)
            where T : struct
        {
            return Marshal.SizeOf(target);
        }

        /// <summary> Packs a <see langword="struct" /> into an array of bytes. </summary>
        /// <typeparam name="T"> typeof(struct). </typeparam>
        /// <param name="target"> The <see langword="struct" /> to pack. </param>
        /// <param name="offset">
        /// The index from which the fields will be packed. The order of declaration matters here.
        /// </param>
        /// <returns>
        /// The <see langword="struct" /> packed in a one dimensional array, and a string to be used
        /// with <see />.
        /// </returns>
        public static Tuple<byte[], string> Pack<T>(this T target, int offset = 0)
            where T : struct
        {
            return Packer.NativePack<T>(target, offset);
        }
    }
}