using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	public static class ConsoleManager
	{
		public static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}
		public static void ClearConsoleLine(int index) {
			Console.SetCursorPosition(0, index);
			ClearCurrentConsoleLine();
		}

		public static void WriteToLine(string text, int lineIndex, ConsoleColor color = ConsoleColor.White)
		{
			Console.ForegroundColor = color;
			Console.SetCursorPosition(0, lineIndex);
			ClearCurrentConsoleLine();
			Console.WriteLine(text);
		}
	}
}
