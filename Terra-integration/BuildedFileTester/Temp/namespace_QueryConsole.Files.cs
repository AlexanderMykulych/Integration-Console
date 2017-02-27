using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terrasoft.TempConfiguration;

namespace QueryConsole.Files {

	#region Class: ConsoleManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\ConsoleManager.cs
		
	*/
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

	#endregion


	#region Class: IntegrationConsoleManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\IntegrationConsoleManager.cs
		
	*/
	public static class IntegrationConsoleManager
	{
		public static int AllProgressLineIndex = 1;
		public static int AllProgressPersent = 0;

		public static int EntityIntegratedFinishLineIndex = 14;
		public static List<string> EntityIntegratedFinish = new List<string>();

		public static int CurrentRequestUrlLineIndex = 3;
		public static int CurrentResponseDataLineIndex = 6;


		public static int CurrentIntegratedEntityLineIndex = 8;
		public static string CurrentIntegratedEntityName;
		public static int CurrentIntegratedEntityProgressPersent = 0;
		public static int CurrentIntegratedEntityProgressErrorPersent = 0;

		public static void WriteToHead(string text, ConsoleColor? color = null) {
			ConsoleManager.WriteToLine(text, 0, color.HasValue ? color.Value : ConsoleColor.White);
		}

		public static void WriteAllProgress()
		{
			ConsoleManager.WriteToLine(string.Format("AllProgress: {0}%", AllProgressPersent), AllProgressLineIndex, ConsoleColor.Green);
		}
		public static void SetAllProgress(int progress)
		{
			AllProgressPersent = progress;
			WriteAllProgress();
		}

		public static void WriteIntegratedEntities()
		{
			if (EntityIntegratedFinish.Any()) {
				string entitiesStr = EntityIntegratedFinish.Aggregate((x, y) => x + ", " + y);
				ConsoleManager.WriteToLine("Integrated Entity: " + entitiesStr, EntityIntegratedFinishLineIndex);
			} else {
				ConsoleManager.WriteToLine("No Entity Integrate", EntityIntegratedFinishLineIndex);
			}
		}

		public static void AddIntegratedEntity(string entityName) {
			EntityIntegratedFinish.Add(string.Format("{0} ({1}%)", entityName, CurrentIntegratedEntityProgressPersent));
			WriteIntegratedEntities();
		}

		public static void WriteCurrentIntegratedEntityProgress()
		{
			ConsoleManager.WriteToLine(string.Format("{0}: {1}%", CurrentIntegratedEntityName, CurrentIntegratedEntityProgressPersent), CurrentIntegratedEntityLineIndex, ConsoleColor.Green);
		}
		public static void WriteCurrentIntegratedEntityErrorProgress()
		{
			ConsoleManager.WriteToLine(string.Format("{0}: {1}%", CurrentIntegratedEntityName, CurrentIntegratedEntityProgressPersent), CurrentIntegratedEntityLineIndex + 1, ConsoleColor.Red);
		}
		public static void SetCurrentIntegratedEntity(string name) {
			CurrentIntegratedEntityName = name;
			CurrentIntegratedEntityProgressPersent = 0;
			WriteCurrentIntegratedEntityProgress();
		}
		public static void SetCurrentIntegratedEntityProgress(int persent)
		{
			CurrentIntegratedEntityProgressPersent = persent;
			WriteCurrentIntegratedEntityProgress();
		}
		public static void SetCurrentIntegratedEntityErrorProgress(int persent)
		{
			CurrentIntegratedEntityProgressErrorPersent = persent;
			WriteCurrentIntegratedEntityErrorProgress();
		}

		public static void WriteRequestUrl(string url) {
			ConsoleManager.ClearConsoleLine(CurrentRequestUrlLineIndex + 1);
			ConsoleManager.WriteToLine(string.Format("Url: {0}", url), CurrentRequestUrlLineIndex);
		}
		public static void ClearRequestUrl() {
			ConsoleManager.ClearConsoleLine(CurrentRequestUrlLineIndex);
			ConsoleManager.ClearConsoleLine(CurrentRequestUrlLineIndex + 1);
		}
		public static void WriteResponseData(int total, int skip, int limit) {
			ConsoleManager.WriteToLine(string.Format("Response: total={0}, skip={1}, limit={2}", total, skip, limit), CurrentResponseDataLineIndex);
		}
		public static void ClearResponseData()
		{
			ConsoleManager.ClearConsoleLine(CurrentResponseDataLineIndex);
		}
		public static void ClearEntityProgress()
		{
			ConsoleManager.ClearConsoleLine(CurrentResponseDataLineIndex);
		}
	}

	#endregion


	#region Class: Pair
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\Pair.cs
		
	*/
	public class Pair<T1, T2>
	{
		public Pair(T1 first, T2 second) {
			First = first;
			Second = second;
		}
		public T1 First { get; set; }
		public T2 Second { get; set; }
	}

	#endregion


	#region Class: TableConsoleParser
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\TableConsoleParser.cs
		
	*/
	public static class TableConsoleParser
	{
		public static string ToTable(this IConsoleTable obj)
		{
			var dict = GetObjFieldValueName(obj);
			return dict.ToStringTable(new[] { "", "" }, x => x.Item1, x => x.Item2);
		}

		public static List<Tuple<string, string>> GetObjFieldValueName(IConsoleTable obj)
		{
			var mapper = obj.GetMapper();
			var resultDict = new List<Tuple<string, string>>();
			foreach (var mapItem in mapper)
			{
				var res = mapItem.Value.Invoke(obj);
				resultDict.Add(new Tuple<string, string>(mapItem.Key, res != null ? res.ToString() : "--/--"));
			}
			return resultDict;
		}
	}

	#endregion


	#region Class: TableRowNameAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\TableRowNameAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property)]
	public class TableRowNameAttribute : Attribute
	{
		public TableRowNameAttribute(string name)
		{
			Name = name;
		}
		public string Name;
	}

	#endregion


	#region Class: Trio
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\Trio.cs
		
	*/
	public class Trio<T1, T2, T3>
	{
		public Trio(T1 first, T2 second, T3 third)
		{
			First = first;
			Second = second;
			Third = third;
		}
		public T1 First { get; set; }
		public T2 Second { get; set; }
		public T3 Third { get; set; }
	}

	#endregion

}