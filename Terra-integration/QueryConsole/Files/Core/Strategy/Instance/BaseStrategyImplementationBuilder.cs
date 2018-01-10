using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Instance
{
	public class BaseStrategyImplementationBuilder: IStrategyImplementationBuilder
	{
		public IStrategyImplementation<T> Build<T>(Dictionary<string, IStrategyStepInfo> steps, IStrategyConfigurator strategyConfigurator)
		{
			var stepBuilder = strategyConfigurator.GetStepBuilder();
			var eventSteps = stepBuilder.GetEventSteps();
			return new BaseStrategyImplementation<T>(steps, eventSteps, stepBuilder.GetCurrentStepName());
		}
	}
}
