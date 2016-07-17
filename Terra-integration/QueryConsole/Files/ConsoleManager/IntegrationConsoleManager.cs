using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
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
}
