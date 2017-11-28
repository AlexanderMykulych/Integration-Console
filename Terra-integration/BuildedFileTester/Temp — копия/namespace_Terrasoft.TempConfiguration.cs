using QueryConsole.Files;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Terrasoft.TempConfiguration {

	#region Class: ConsoleInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\ConsoleInfo.cs
		
	*/
	public class ConsoleInfo : IConsoleTable
	{
		public ConsoleInfo(Action refresh)
		{
			Refresh = refresh;
		}
		public Action Refresh;


		private string _integrationStatus;
		public string IntegrationStatus
		{
			get
			{
				return _integrationStatus;
			}
			set
			{
				_integrationStatus = value;
				Console.Clear();
				Refresh();
			}
		}

		private int _progress;
		public int Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				_progress = value;
				Refresh();
			}
		}
		private string _requestStatus;
		public string RequestStatus
		{
			get
			{
				return _requestStatus;
			}
			set
			{
				_requestStatus = value;
			}
		}

		private string _url;
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_response = "";
				_url = value;
				RequestStatus = "Push Request";
				Refresh();
			}
		}

		private string _response;
		public string Response
		{
			get
			{
				return _response;
			}
			set
			{
				if (_response == value)
				{
					return;
				}
				_response = value;
				RequestStatus = "Process Response";
				Console.Clear();
				Refresh();
			}
		}

		private string _currentEntityName;
		public string CurrentEntityName
		{
			get
			{
				return _currentEntityName;
			}
			set
			{
				_currentEntityName = value;
				Refresh();
			}
		}

		private Dictionary<string, Trio<int, int, int>> _entityProgress = new Dictionary<string, Trio<int, int, int>>();
		public Dictionary<string, Trio<int, int, int>> EntityProgress
		{
			get
			{
				return _entityProgress;
			}
			set
			{
				_entityProgress = value;
				Refresh();
			}
		}

		private int _entityErrorProgress;
		public int EntityErrorProgress
		{
			get
			{
				return _entityErrorProgress;
			}
			set
			{
				_entityErrorProgress = value;
				Refresh();
			}
		}

		private int _summaryEntityCount;
		public int SummaryEntityCount
		{
			get
			{
				return _summaryEntityCount;
			}
			set
			{
				_summaryEntityCount = value;
				Refresh();
			}
		}

		private int _mappingError;
		public int MappingError {
			get {
				return _mappingError;
			}
			set {
				_mappingError = value;
				Refresh();
			}
		}

		public Dictionary<string, Func<object, object>> GetMapper()
		{
			return new Dictionary<string, Func<object, object>>() {
				{ "Integration Status", x => ((ConsoleInfo)x).IntegrationStatus },
				{ "Progress", x => ((ConsoleInfo)x).Progress + "%" },
				{ "Request Status", x => ((ConsoleInfo)x).RequestStatus },
				{ "Url", x => ((ConsoleInfo)x).Url },
				{ "Response", x => ((ConsoleInfo)x).Response },
				{ "Entity Name", x => {
					var value = ((ConsoleInfo)x).CurrentEntityName;
					if(string.IsNullOrEmpty(value))
						return "";
					return string.Format("{0} - {1}%", value, GetPersent(_entityProgress[value].First, _entityProgress[value].Second));
					}
				},
				{ "Entity Progress", x => ((ConsoleInfo)x).EntityProgress.Select(z => string.Format("{0} ({1} - {2} - {3})", z.Key, z.Value.First, z.Value.Second, z.Value.Third)).Aggregate((z,y) =>  z + "\n" + y)},
				{ "Mapping Error", x => ((ConsoleInfo)x).MappingError },
				{ "Save Error Count", x => ((ConsoleInfo)x).EntityErrorProgress },
				{ "Total Count", x => ((ConsoleInfo)x).SummaryEntityCount }
			};
		}

		public static int GetPersent(int all, int part)
		{
			return (part * 100) / all;
		}
	}

	#endregion


	#region Class: IntegrationConsole
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\IntegrationConsole.cs
		
	*/
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
			try {
				if (!string.IsNullOrEmpty(ConsoleInfo.CurrentEntityName) && name.ToLower() == ConsoleInfo.CurrentEntityName.ToLower())
				{
					ConsoleInfo.EntityProgress[ConsoleInfo.CurrentEntityName].Second++;
					RecalculateAllProgress();
				}
			} catch(Exception e) {

			}
		}

		public static void EntityIntegratedError(string name)
		{
			try {
				if (!string.IsNullOrEmpty(ConsoleInfo.CurrentEntityName) && name.ToLower() == ConsoleInfo.CurrentEntityName.ToLower()) {
					ConsoleInfo.EntityProgress[ConsoleInfo.CurrentEntityName].Third++;
				}
			} catch (Exception) {
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
			Console.WriteLine("Ok");
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

	#endregion


	#region Class: TableParser
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\IntegrationConsole.cs
		
	*/

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

	#endregion


	#region Interface: IConsoleTable
	/*
		Project Path: ..\..\..\QueryConsole\Files\ConsoleManager\IConsoleTable.cs
		
	*/
	public interface IConsoleTable
	{
		Dictionary<string, Func<object, object>> GetMapper();
	}

	#endregion

}