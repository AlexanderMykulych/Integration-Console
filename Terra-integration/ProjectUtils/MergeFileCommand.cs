using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;

namespace ProjectUtils
{
	public class MergeFileCommand: ConsoleCommand
	{
		private const int Success = 0;
		private const int Failure = 2;
		public string InputFolderPath;
		public string OutputFolderPath;
		public string Namespace;
		public MergeFileCommand()
		{
			IsCommand("MergeFile", "Merge QueryConsole Project FIle to One");

			HasRequiredOption("ip|inputPath=", "The full input folder path", p => InputFolderPath = p);
			HasRequiredOption("op|outputPath=", "The full output folder path", p => OutputFolderPath = p);
			HasRequiredOption("n|namespace=", "The namespace", p => Namespace = p);
		}

		public override int Run(string[] remainingArguments)
		{
			try
			{
				var builder = new ProjectBuilder(InputFolderPath, OutputFolderPath, false);
				builder.WithOneNameSpace(Namespace);
				var result = builder.Run();
				Console.Out.WriteLine("Result:");
				result.ForEach(x => Console.Out.WriteLine(x));;
				return Success;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);

				return Failure;
			}
		}
	}
}
