using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProjectUtils
{
	internal class Program
	{
		private static void Main()
		{
			var builder = new ProjectBuilder(@"..\..\..\QueryConsole", @"..\..\..\BuildedFileTester\Temp");
			builder.Run();
		}
	}
}