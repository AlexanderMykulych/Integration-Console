using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface ISetting
	{
		T2 Select<T1, T2>(Func<T1, T2> getter);
		IEnumerable<T2> SelectFromList<T1, T2>(Func<T1, T2> getter);
		T1 Select<T1>();
		IEnumerable<T1> SelectFromList<T1>();
	}
}
