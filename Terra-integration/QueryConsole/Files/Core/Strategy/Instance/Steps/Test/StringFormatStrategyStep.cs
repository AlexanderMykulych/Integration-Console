using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class StringFormatStrategyStep: BaseStrategyStep
	{
		private readonly string _format;

		public StringFormatStrategyStep(string format)
		{
			_format = format;
		}
		public override void Execute(object input)
		{
			var str = (string) input;
			if (str.StartsWith("S"))
			{
				Subscriber.Execute("Success", string.Format(_format, input));
			}
			else
			{
				Subscriber.Execute("Error", string.Format(_format, input));
			}
		}
	}
}
