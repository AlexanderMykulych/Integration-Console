using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.Configuration.FileImport
{
	public class StorageRow: IStorageRow
	{
		private Dictionary<string, IStorageCellValue> _cells = new Dictionary<string, IStorageCellValue>();
		public StorageRow(Dictionary<string, string> initDictionary)
		{
			foreach (var item in initDictionary)
			{
				_cells.Add(item.Key, new StorageCellValue(item.Value));
			}
		}
		public StorageRow(Dictionary<string, IStorageCellValue> initDictionary)
		{
			_cells = initDictionary;
		}

		public static StorageRow Creator<T>(IEnumerable<T> enumerable, Func<T, string> keyFunc, Func<T, string> valueFunc)
		{
			var dict = new Dictionary<string, IStorageCellValue>();
			foreach (var item in enumerable)
			{
				dict.Add(keyFunc(item), new StorageCellValue(valueFunc(item)));
			}
			return new StorageRow(dict);
		}
		public IStorageCellValue GetByKey(string key)
		{
			if (_cells.ContainsKey(key))
			{
				return _cells[key];
			}
			return null;
		}
	}
}
