using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectUtilsClassLibrary;

namespace ProjectUtils
{
	internal class Program
	{
		private static void Main()
		{
			Console.WriteLine("Укажите команду:\n1. Собрать проект в файл\n2. Создать проект из файла");
			var res = int.Parse(Console.ReadLine());
			if (res == 1)
			{
				var builder = new ProjectBuilder(@"..\..\..\QueryConsole\Files", @"..\..\..\BuildedFileTester\Temp", false);
				builder.Run();
			}
			else
			{
				var builder = new ReverseProjectBuilder(@"..\..\..\BuildedFileTester\Temp\namespace_Terrasoft.TsConfiguration.cs", @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\QueryConsole");
				builder.Run();
			}
			Console.WriteLine("Работа закончена!");
			Console.ReadKey();
		}
	    private static void Main1()
	    {
			var builder = new ReverseProjectBuilder(@"..\..\..\BuildedFileTester\Temp\namespace_Terrasoft.TsConfiguration.cs", @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\QueryConsole");
			builder.Run();
			Console.ReadKey();
	    }
    }
}