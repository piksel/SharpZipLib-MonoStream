using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Versioning;
#if NETCORE2
using System.Runtime.InteropServices;
using RI = System.Runtime.InteropServices.RuntimeInformation;
using RE = System.Runtime.InteropServices.RuntimeEnvironment;
#endif

namespace MonoBugTest
{
	public static class Program
	{
        const string helloWorldZippedBase64 = "H4sIAAAAAAAAA/NIzcnJVwAAwPwt6gYAAAAfiwgAAAAAAAADUyjPL8pJUQQAqd+XkgcAAAA=";
        const string expectedResult = "Hello  world!";
        static byte[] helloWorldZippedBytes;
        static ConsoleColor defaultColor;

		public static void Main()
		{
			var framework = Assembly
				.GetEntryAssembly()?
				.GetCustomAttribute<TargetFrameworkAttribute>()?
				.FrameworkName;

            var procArch = Environment.Is64BitProcess ? "X64" : "X86";
            var osArch = Environment.Is64BitOperatingSystem ? "X64" : "X86";

#if NETCORE2
            Console.Title = $"MonoStream {RI.FrameworkDescription}";
            Console.WriteLine($"Framework: {framework} ({RI.FrameworkDescription}) CLR: {RE.GetSystemVersion()}");
			Console.WriteLine($"OS: {Environment.OSVersion} ({RI.OSDescription.Trim()}) {procArch} {RI.OSArchitecture} {RI.ProcessArchitecture}");
#else
            Console.WriteLine($"Framework: {framework} CLR: {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion} {procArch} {osArch}");
#endif
            Console.WriteLine();

            helloWorldZippedBytes = Convert.FromBase64String(helloWorldZippedBase64);

            defaultColor = Console.ForegroundColor;

            test(ms => new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress));
            test(ms => new ICSharpCode.SharpZipLib.GZip.GZipInputStream(ms));

#if !CI_BUILD
            Console.Write("Press any key to exit...");
			Console.ReadKey();
#endif
        }

        private static void test<TStream>(Func<MemoryStream, TStream> init) where TStream: Stream
        {
            var type = typeof(TStream);
            var assembly = type.Assembly.GetName();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{type.FullName}");
            Console.ForegroundColor = defaultColor;
            Console.Write($" ({assembly.Name}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" v{assembly.Version}");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine($"):");


            using (var buffer = new MemoryStream(helloWorldZippedBytes))
            using (var gz = init(buffer))
            {
                int res;
                int read = 0;
                var sb1 = new StringBuilder();
                var sb2 = new StringBuilder();
                var reader = new StreamReader(gz);
                while ((res = reader.Read()) >= 0)
                {
                    sb1.Append((char)(res));
                    sb2.Append($"{res:x2} ");
                    read++;
                }

                var result = sb1.ToString();
                var passed = result == expectedResult;

                Console.Write($"Result: ");
                Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write($"{result,-13}");
                Console.ForegroundColor = defaultColor;
                Console.WriteLine($" {sb2,-39} ({read,2})\n");
            }
        }
    }
}
