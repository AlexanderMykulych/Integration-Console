using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;
using ProjectUtilsClassLibrary;

namespace ProjectUtils
{
	public class SplitFileCommand: ConsoleCommand
	{
		private const int Success = 0;
		private const int Failure = 2;
		public string InputFilePath;
		public string OutputFolderPath;
		public SplitFileCommand()
		{
			IsCommand("SplitFile", "Split file to project");

			HasRequiredOption("ip|inputPath=", "The full input folder path", p => InputFilePath = p);
			HasRequiredOption("op|outputPath=", "The full output folder path", p => OutputFolderPath = p);
		}
		public override int Run(string[] remainingArguments)
		{
			
			try
			{
				var builder = new ReverseProjectBuilder(InputFilePath, OutputFolderPath);
				builder.Run();
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
