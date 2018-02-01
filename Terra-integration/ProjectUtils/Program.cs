using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using ManyConsole;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectUtilsClassLibrary;

namespace ProjectUtils
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			var commands = GetCommands();

			return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
		}
	    public static IEnumerable<ConsoleCommand> GetCommands()
		{
			return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
		}
	}
}