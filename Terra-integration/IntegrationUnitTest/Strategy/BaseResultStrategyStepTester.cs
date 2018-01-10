using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.Strategy
{
	public class BaseResultStrategyStepTester: BaseStrategyStep
	{
		private Action<object> _onExecute;

		public BaseResultStrategyStepTester(Action<object> onExecute)
		{
			_onExecute = onExecute;
		}
		public override void Execute(object input)
		{
			_onExecute(input);
		}
	}
}
