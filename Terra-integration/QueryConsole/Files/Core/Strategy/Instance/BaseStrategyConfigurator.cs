using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseStrategyConfigurator: IStrategyConfigurator
	{
		private readonly IStepBuilder _stepBuilder;
		public BaseStrategyConfigurator(IStepBuilder stepBuilder)
		{
			_stepBuilder = stepBuilder;
		}
		public IStrategyConfigurator InitStep(string stepName, Action<IStepBuilder> stepConfigurator)
		{
			_stepBuilder.InitCurrentStepName(stepName);
			stepConfigurator(_stepBuilder);
			return this;
		}

		public IStepBuilder GetStepBuilder()
		{
			return _stepBuilder;
		}
	}
}
