using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TempConfiguration
{
	public interface IConsoleTable
	{
		Dictionary<string, Func<object, object>> GetMapper();
	}
}
