using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IStrategy<T>
	{
		//IStrategy<T> AddStep<T1, T2>(string name, Func<IStrategyStep> factoryFunc,
		//	Func<IStrategyStep, Func<T1, T2>> execMethod)
		//	where T1 : class
		//	where T2 : class;

		IStrategy<T> ConfigurateStrategy(Action<IStrategyConfigurator> builderConfigurator);

		IStrategyImplementation<T> BuildImplementation();
	}
}
