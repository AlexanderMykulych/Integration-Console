using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IStrategyImplementationBuilder
	{
		IStrategyImplementation<T> Build<T>(Dictionary<string, IStrategyStepInfo> steps, IStrategyConfigurator strategyConfigurator);

	}
}
