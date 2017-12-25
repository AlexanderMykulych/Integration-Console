using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class IntegerStrategyStep: BaseStrategyStep
	{
		private readonly int _plus;

		public IntegerStrategyStep(int plus)
		{
			_plus = plus;
		}
		
		public override void Execute(object input)
		{
			var integer = (int)input;
			Subscriber.Execute("Success", integer + _plus);
		}
	}
}
