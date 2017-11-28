using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[MacrosImport("GetOnlyNumber")]
	public class GetOnlyNumberMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				if (x != null && x is string)
				{
					var regex = new Regex(@"^-?\d+(?:\,\d+)?(?:\.\d+)?");
					Match match = regex.Match((string)x);
					if (match.Success)
					{
						return decimal.Parse(match.Value);
					}
				}
				return x;
			};
		}
	}
}
