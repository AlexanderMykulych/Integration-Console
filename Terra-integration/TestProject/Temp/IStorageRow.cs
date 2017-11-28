using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.Configuration.FileImport
{
	public interface IStorageRow
	{
		IStorageCellValue GetByKey(string key);
	}
}
