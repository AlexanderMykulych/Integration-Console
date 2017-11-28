using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[MacrosExport("DateFormat")]
	public class DateFormatMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				DateTime result;
				if (x != null && x is string && DateTime.TryParse((string)x, out result))
				{
					return result.ToString("MM-dd-yyyy");
				}
				return x;
			};
		}
	}
}
