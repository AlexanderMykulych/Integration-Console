using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[MacrosImport("EpochDateTime")]
	public class EpochDateTime : IMacrosCreator
	{
		private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public Func<object, object> Create()
		{
			return (x) =>
			{
				if (x != null && x is string)
				{
					long time;
					if (long.TryParse((string) x, out time))
					{
						return _epoch.AddSeconds(time);
					}
				}
				return x;
			};
		}
	}
}
