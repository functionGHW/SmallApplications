using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SimpleCmdLineParser;

namespace filehasher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || "--help".Equals(args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("    fileshasher --file <file_path> [--alg <alg_name>]\r\n");
                Console.WriteLine("Arguments:");
                Console.WriteLine("    --file\t file path to compute hash");
                Console.WriteLine("    --alg\t optional, name ofhash algorithm, i.e. md5(default), sha1, sha256...");

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

            if (!File.Exists(cmdArgs.InputFile))
            {
                Console.Error.WriteLine("file not found");
                Environment.Exit(-1);
            }
            /*
             * 支持的算法名称有：md5, sha1, sha256等，详情请查阅msdn文档关于HashAlgorithm.Create()的说明
             */
            var hasher = HashAlgorithm.Create(cmdArgs.Alg);
            if (hasher == null)
            {
                Console.Error.WriteLine("invalid hash alg");
                Environment.Exit(-1);
            }
            var sb = new StringBuilder(32);
            using (var fs = File.Open(cmdArgs.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var hashBytes = hasher.ComputeHash(fs);
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            Console.WriteLine(sb.ToString());
        }
    }

    class CmdArgs
    {
        [Argument("--file")]
        public string InputFile { get; set; }

        [Argument("--alg", Optional = true)]
        public string Alg { get; set; } = "MD5"; // 缺省使用md5算法
    }
}
