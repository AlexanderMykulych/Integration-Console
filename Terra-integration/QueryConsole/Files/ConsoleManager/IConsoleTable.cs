using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	public interface IConsoleTable
	{
		Dictionary<string, Func<object, object>> GetMapper();
	}
}
