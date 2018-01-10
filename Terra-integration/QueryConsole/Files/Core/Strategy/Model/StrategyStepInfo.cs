using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model
{
	public interface IStrategyStepInfo
	{
		Func<IStrategyStep> FactoryFunc { get; set; }
	}
}
