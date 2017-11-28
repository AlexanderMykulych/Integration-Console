using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.Configuration.FileImport
{
	public class StorageCellValue: IStorageCellValue
	{
		private string _value;
		public StorageCellValue(string value)
		{
			_value = value;
		}
		public string GetValue()
		{
			return _value;
		}
	}
}
