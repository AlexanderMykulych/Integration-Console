using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseStrategyStepInfo: IStrategyStepInfo
	{
		public BaseStrategyStepInfo(Func<IStrategyStep> factoryFunc)
		{
			FactoryFunc = factoryFunc;
		}
		public Func<IStrategyStep> FactoryFunc { get; set; }

	}
}
