using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
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
}
