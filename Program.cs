using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Versioning;

namespace MonoBugTest
{
	public static class Program
	{
		public static void Main()
		{
			var framework = Assembly
				.GetEntryAssembly()?
				.GetCustomAttribute<TargetFrameworkAttribute>()?
				.FrameworkName;

			Console.WriteLine($"OS: {Environment.OSVersion} (ubuntu 18.04 x64)");
			Console.WriteLine($"Framework: {framework}");
			Console.WriteLine();

            var helloWorldZippedBytes = Convert.FromBase64String("H4sIAAAAAAAAA/NIzcnJVwAAwPwt6gYAAAAfiwgAAAAAAAADUyjPL8pJUQQAqd+XkgcAAAA=");

			using (var buffer = new MemoryStream(helloWorldZippedBytes))
			using (var gz = new System.IO.Compression.GZipStream(buffer, System.IO.Compression.CompressionMode.Decompress))
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
				
				Console.WriteLine("{0,-16} <- by Microsoft", sb1.ToString());
				Console.WriteLine($"Bytes: {sb2}");
				Console.WriteLine($"Read count: {read}");
			}


			using (var buffer = new MemoryStream(helloWorldZippedBytes))
			using (var gz = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(buffer))
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
				Console.WriteLine("{0,-16} <- by SharpZipLib", sb1.ToString());
				Console.WriteLine($"Bytes: {sb2}");
				Console.WriteLine($"Read count: {read}");
			}

			Console.Write("Press any key to exit...");
			Console.ReadKey();
		}
	}
}
