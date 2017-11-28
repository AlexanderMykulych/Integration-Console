using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace ProjectUtilsClassLibrary
{
	public class ReverseProjectBuilderBenchmark
	{
		public static string InputFile;
		public static string OutputFolder;
		public ReverseProjectBuilderBenchmark()
		{
		}
		[Benchmark]
		public void Run()
		{
			var builder = new ReverseProjectBuilder(InputFile, OutputFolder);
			builder.Run();
		}
	}
}
