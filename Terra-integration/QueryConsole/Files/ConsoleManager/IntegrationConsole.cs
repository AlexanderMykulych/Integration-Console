using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	public static class IntegrationConsole
	{
		public static ConsoleInfo ConsoleInfo = new ConsoleInfo(() => WriteResult());
		public static void StartIntegrate() {
			ConsoleInfo.IntegrationStatus = "In Progress";
		}
		public static void AddEntityProgress(string name, int allCount, int startCount = 0) {
			ConsoleInfo.EntityProgress.Add(name, new Trio<int, int, int>(allCount, startCount, 0));
			ConsoleInfo.SummaryEntityCount += allCount;
		}
		public static void SetCurrentEntity(string name) {
			if(ConsoleInfo.CurrentEntityName != null && name.ToLower() == ConsoleInfo.CurrentEntityName.ToLower()) {
				return;
			}
			ConsoleInfo.CurrentEntityName = name;
		}
		public static void EndIntegrated() {
			ConsoleInfo.IntegrationStatus = "Finish";
		}
		public static void EntityIntegratedSuccess(string name) {
			if (name.ToLower() == ConsoleInfo.CurrentEntityName.ToLower())
			{
				ConsoleInfo.EntityProgress[ConsoleInfo.CurrentEntityName].Second++;
				RecalculateAllProgress();
			}
		}

		public static void EntityIntegratedError(string name)
		{
			if (name.ToLower() == ConsoleInfo.CurrentEntityName.ToLower())
			{
				ConsoleInfo.EntityProgress[ConsoleInfo.CurrentEntityName].Third++;
			}
		}
		

		public static void RecalculateAllProgress() {
			ConsoleInfo.Progress = (ConsoleInfo.EntityProgress.Sum(x => x.Value.Second) * 100) / ConsoleInfo.SummaryEntityCount;
		}
		public static void SetCurrentRequestUrl(string url) {
			ConsoleInfo.Url = url;
		}

		public static void SetCurrentResponseSucces(int total, int skip, int limit) {
			ConsoleInfo.Response = string.Format("total={0} skip={1} limit={2}", total, skip, limit);
		}

		public static int GetPersent(string name)
		{
			return (ConsoleInfo.EntityProgress[name].Second * 100) / ConsoleInfo.EntityProgress[name].First;
		}
		public static int GetPersentError(string name)
		{
			return (ConsoleInfo.EntityProgress[name].Third * 100) / ConsoleInfo.EntityProgress[name].Third;
		}

		public static void WriteResult() {
			Console.SetCursorPosition(0, 0);
			Console.WriteLine(ConsoleInfo.ToTable());
		}

		public static void AddMappingError() {
			ConsoleInfo.MappingError++;
		}
	}

	public static class TableParser
	{
		public static string ToStringTable<T>(this IEnumerable<T> values,
			string[] columnHeaders,
			params Func<T, object>[] valueSelectors
			)
		{
			return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
		}

		public static string ToStringTable<T>(
		  this T[] values,
		  string[] columnHeaders,
		  params Func<T, object>[] valueSelectors)
		{
			Debug.Assert(columnHeaders.Length == valueSelectors.Length);

			var arrValues = new string[values.Length + 1, valueSelectors.Length];

			// Fill headers
			for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
			{
				arrValues[0, colIndex] = columnHeaders[colIndex];
			}

			// Fill table rows
			for (int rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
			{
				for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
				{
					arrValues[rowIndex, colIndex] = valueSelectors[colIndex]
					  .Invoke(values[rowIndex - 1]).ToString();
				}
			}

			return ToStringTable(arrValues);
		}

		public static string ToStringTable(this string[,] arrValues)
		{
			int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
			var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

			var sb = new StringBuilder();
			for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
			{
				for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
				{
					// Print cell
					string cell = arrValues[rowIndex, colIndex];
					if (cell.Contains('\n'))
					{
						var rows = cell.Split('\n');
						foreach (var row in rows)
						{
							var newRow = row.PadRight(maxColumnsWidth[colIndex]);
							sb.Append(" | ");
							sb.Append(newRow);
							sb.AppendLine(" | ");
							sb.Append(" | ".PadRight(maxColumnsWidth[colIndex - 1] + 3));
						}
					}
					else
					{
						cell = cell.PadRight(maxColumnsWidth[colIndex]);
						sb.Append(" | ");
						sb.Append(cell);
					}
				}

				// Print end of line
				sb.Append(" | ");
				sb.AppendLine();

				// Print splitter
				sb.AppendFormat(" |{0}| ", headerSpliter);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		private static int[] GetMaxColumnsWidth(string[,] arrValues)
		{
			var maxColumnsWidth = new int[arrValues.GetLength(1)];
			int colIndex = 0;
			for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
			{
				var val = arrValues[rowIndex, colIndex];
				int newLength = 0;
				if (val.Contains('\n')) {
					newLength = val.Split('\n').Max(x => x.Length);
				} else {
					newLength = val.Length;
				}
				int oldLength = maxColumnsWidth[colIndex];

				if (newLength > oldLength)
				{
					maxColumnsWidth[colIndex] = newLength;
				}
			}
			maxColumnsWidth[1] = 100;
			return maxColumnsWidth;
		}
	}

}
