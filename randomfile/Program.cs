using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleCmdLineParser;

namespace randomfile
{
    class Program
    {
        private const long OneKB = 1024;
        private const long OneMB = 1024 * OneKB;
        private const long OneGB = 1024 * OneMB;
        private const long OneTB = 1024 * OneGB;

        static void Main(string[] args)
        {
            if (args.Length == 0 || "--help".Equals(args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("    randomfile --size <file_size> [--filename <file_name>]\r\n");
                Console.WriteLine("Arguments:");
                Console.WriteLine("    --size\t file size to generate, default unit is B(yte), , i.e. 512, 1000B, 200MB");
                Console.WriteLine("    --filename\t optional, output filename, default is randomfile.tmp");

                return;
            }

            CmdArgs cmdArgs = null;
            try
            {
                cmdArgs = SimpleCmdLineParser.SimpleCmdLineParser.Parse<CmdArgs>(args);
            }
            catch (ParserException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
            }

            if (string.IsNullOrEmpty(cmdArgs.Size))
            {
                Console.Error.WriteLine("no file size");
                return;
            }
            string size = cmdArgs.Size.Trim().ToUpperInvariant();
            long unitSize = 1;
            string num = "";
            if (size.EndsWith("TB"))
            {
                unitSize = OneTB;
                num = size.Substring(0, size.Length - 2);
            }
            else if (size.EndsWith("GB"))
            {
                unitSize = OneGB;
                num = size.Substring(0, size.Length - 2);
            }
            else if (size.EndsWith("MB"))
            {
                unitSize = OneMB;
                num = size.Substring(0, size.Length - 2);
            }
            else if (size.EndsWith("KB"))
            {
                unitSize = OneKB;
                num = size.Substring(0, size.Length - 2);
            }
            else if (size.EndsWith("B"))
            {
                unitSize = 1;
                num = size.Substring(0, size.Length - 1);
            }
            if (string.IsNullOrEmpty(num) || !long.TryParse(num, out var sizeNum) || sizeNum < 0)
            {
                Console.Error.WriteLine("invalid file size: " + cmdArgs.Size);
                return;
            }

            sizeNum = sizeNum * unitSize;

            var rand = new Random();
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            long fillCount = (sizeNum + bufferSize - 1) / bufferSize;
            using (var fs = File.Create(cmdArgs.Filename))
            {
                fs.SetLength(sizeNum);
                for (int i = 0; i < fillCount; i++)
                {
                    rand.NextBytes(buffer);
                    fs.Write(buffer, 0, bufferSize);
                }
                fs.SetLength(sizeNum);
            }
        }
    }
    class CmdArgs
    {
        [Argument("--size")]
        public string Size { get; set; }

        [Argument("--filename", Optional = true)]
        public string Filename { get; set; } = "randomfile.tmp";
    }
}
