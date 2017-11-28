using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[MacrosExport("IntToLow")]
	public class IntToLowMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				decimal decValue = 0;
				if (x != null)
				{
					if (x is string && decimal.TryParse((string)x, out decValue))
					{
						return decimal.ToInt64(decValue);
					}
					if (x is decimal)
					{
						return decimal.ToInt64((decimal)x);
					}
				}
				return x;
			};
		}
	}
}
