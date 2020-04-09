using CommandLine;
using OPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OPack.Cli
{
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
            var bytes = Packer.Pack(
                opts.PackFormat,
                offset,
                opts.Input.Split(" ").
                Select(x => long.Parse(x, NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture)));
            var output = bytes.Select(x => x.ToString("X2", CultureInfo.CurrentCulture));
            Console.WriteLine(output);
        }

#pragma warning disable CA1812

        private class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input number(s) to pack, separated by spaces.")]
            public string Input { get; set; }

            [Option('o', "offset", Required = false, HelpText = "Pack from offset.")]
            public int? Offset { get; set; }

            [Option('f', "format", Required = true, HelpText = "Pack format.")]
            public string PackFormat { get; set; }
        }

#pragma warning restore CA1812
    }
}