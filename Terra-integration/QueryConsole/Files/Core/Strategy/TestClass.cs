using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.DB;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration
{
	public class TestClass
	{
		public void Test()
		{
			var importConfig = ObjectFactory.Get<IStrategyConfigurator>()
				.InitStep("Input",
					stepBuilder => stepBuilder.OnSuccessDo("GetOrCreateEntity",
						getOrCreateStep => getOrCreateStep.OnDo(new List<string>()
							{
								"Create",
								"Update"
							}, "Mapp", mappStep => mappStep.OnSuccessDo("SaveEntity")
						)
					)
				);

			var importStrategy = ObjectFactory.Get<IStrategyImplementationBuilder>()
				.Build<int>(new Dictionary<string, IStrategyStepInfo>()
				{
					{ "Input", new BaseStrategyStepInfo(() => new IntegerStrategyStep(1)) },
					{ "GetOrCreateEntity", new BaseStrategyStepInfo(() => new IntegerStrategyStep(4)) },
					{ "Mapp", new BaseStrategyStepInfo(() => new IntegerStrategyStep(8)) },
					{ "SaveEntity", new BaseStrategyStepInfo(() => new IntegerStrategyStep(16)) },
				}, importConfig);

			importStrategy.Execute(5);
		}
	}
}
