namespace OPack.Cli
{
    using CommandLine;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal class Program
    {
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Options parse error.");
        }

        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        private static void RunOptions(Options opts)
        {
            int offset = 0;
            if (opts.Offset.HasValue)
            {
                offset = opts.Offset.Value;
            }
            var bytes = new Packer().Pack(
                opts.PackFormat,
                offset,
                opts.Input);

            if (opts.MustUnpack)
            {
                var objects = new Packer().Unpack(opts.PackFormat, offset, bytes);
                Console.Write('(');
                for (int i = 0; i < objects.Length; i++)
                {
                    Console.Write($"{objects[i]}");
                    Console.Write(',');
                }
                Console.Write(')');
                Console.Write(Environment.NewLine);
            }
            else
            {
                var output = new StringBuilder();
                foreach (var item in bytes)
                {
                    output.Append(@"\x").Append(item.ToString("X", CultureInfo.CurrentCulture));
                }

                Console.WriteLine($"b'{output}'");
            }
        }

#pragma warning disable CA1812

        private class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input number(s) to pack, separated by spaces.")]
            public IEnumerable<double> Input { get; set; }

            [Option('u', "unpack", Required = false, HelpText = "Display result after unpacking, not only packing.")]
            public bool MustUnpack { get; set; }

            [Option('o', "offset", Required = false, HelpText = "Pack from offset.")]
            public int? Offset { get; set; }

            [Option('f', "format", Required = true, HelpText = "Pack format.")]
            public string PackFormat { get; set; }
        }

#pragma warning restore CA1812
    }
}