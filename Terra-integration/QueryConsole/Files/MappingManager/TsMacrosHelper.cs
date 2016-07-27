using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public enum MacrosType
	{
		Rule,
		OverRule
	}
	public static class TsMacrosHelper
	{
		public static Dictionary<string, Func<object, object>> MacrosDictImport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => YearIntegerToDateTime(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => TimeSpanToDate(x) },
			{ "TimeSpanToDateTime", x => TimeSpanToDateTime(x) },
			{ "ToDateTime", x => ToDateTime(x) }
		};
		public static Dictionary<string, Func<object, object>> MacrosDictExport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => DateTimeToYearInteger(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => DateToTimeSpan(x) },
			{ "TimeSpanToDateTime", x => DateToTimeSpan(x) }
		};
		public static Dictionary<string, Func<object, object>> OverMacrosDictImport = new Dictionary<string, Func<object, object>>() {
			{ "ConvertJson", x => ConvertStringToJson(x)}
		};
		public static Dictionary<string, Func<object, object>> OverMacrosDictExport = new Dictionary<string, Func<object, object>>() {
			{ "ConvertJson", x => ConvertJsonToString(x)}
		};

		public static object GetMacrosResultImport(string macrosName, object value, MacrosType type = MacrosType.Rule)
		{
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictImport.ContainsKey(macrosName) && MacrosDictImport[macrosName] != null)
					{
						return MacrosDictImport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictImport.ContainsKey(macrosName) && OverMacrosDictImport[macrosName] != null)
					{
						return OverMacrosDictImport[macrosName](value);
					}
					return value;
				default:
					return value;
			}

		}
		public static object GetMacrosResultExport(string macrosName, object value, MacrosType type = MacrosType.Rule)
		{
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictExport.ContainsKey(macrosName) && MacrosDictExport[macrosName] != null)
					{
						return MacrosDictExport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictExport.ContainsKey(macrosName) && OverMacrosDictExport[macrosName] != null)
					{
						return OverMacrosDictExport[macrosName](value);
					}
					return value;
				default:
					return value;
			}
		}

		#region Macros: Import
		public static Func<object, object> DateTimeToYearInteger = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (DateTime.TryParse((string)x, out dateTimeResult))
				{
					return dateTimeResult.Year;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Year;
			}
			return x;
		};
		public static Func<object, object> DateWithoutTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (DateTime.TryParse((string)x, out dateTimeResult))
				{
					return dateTimeResult.Date;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Date;
			}
			return x;
		};

		public static DateTime StartEpohDate = Convert.ToDateTime("1 January 1970");
		public static Func<object, object> TimeSpanToDate = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan).Date;
				}
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan).Date;
			}
			return x;
		};
		public static Func<object, object> TimeSpanToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan);
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return date;
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ConvertJsonToString = (x) =>
		{
			if (x == null)
				return null;
			if (x is JToken)
			{
				return ((JToken)x).ToString();
			}
			return x;
		};
		public static Func<object, object> ConvertStringToJson = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				return JToken.Parse((string)x);
			}
			return x;
		};
		#endregion

		#region Macros: Export
		public static Func<object, object> YearIntegerToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var year = 0;
				if (int.TryParse((string)x, out year))
				{
					return new DateTime(year, 1, 1);
				}
			}
			if (x is int)
			{
				return new DateTime((int)x, 1, 1);
			}
			if (x is Int64)
			{
				if ((Int64)x > Int32.MaxValue)
				{
					return new DateTime(Int32.MaxValue - 1, 1, 1);
				}
				return new DateTime(Convert.ToInt32((Int64)x), 1, 1);
			}
			return x;
		};
		public static Func<object, object> DateToTimeSpan = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return (date - StartEpohDate).TotalMilliseconds;
				}
			}
			if (x is DateTime)
			{
				return (((DateTime)x) - StartEpohDate).TotalMilliseconds;
			}
			return x;
		};
		#endregion
	}
}
