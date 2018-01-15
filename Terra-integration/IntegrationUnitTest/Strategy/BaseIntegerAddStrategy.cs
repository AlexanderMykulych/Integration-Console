using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using Terrasoft.TsIntegration.Configuration;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;
using NUnit.Framework;

namespace IntegrationUnitTest.Strategy
{
	[TestFixture]
	[Category("Strategy")]
	public class BaseAddStrategy
	{
		[Test]
		public void AddIntegerStrategy()
		{
			var importConfig = ObjectFactory.Get<IStrategyConfigurator>()
				.InitStep("Input",
					stepBuilder => stepBuilder.OnSuccessDo("GetOrCreateEntity",
						getOrCreateStep => getOrCreateStep.OnSuccessDo("Mapp",
							mappStep => mappStep.OnSuccessDo("SaveEntity", saveEntityStep => saveEntityStep.OnSuccessDo("Test"))
						)
					)
				);

			int result = 0;
			var importStrategy = ObjectFactory.Get<IStrategyImplementationBuilder>()
				.Build<int>(new Dictionary<string, IStrategyStepInfo>()
				{
					{ "Input", new BaseStrategyStepInfo(() => new IntegerStrategyStep(1)) },
					{ "GetOrCreateEntity", new BaseStrategyStepInfo(() => new IntegerStrategyStep(4)) },
					{ "Mapp", new BaseStrategyStepInfo(() => new IntegerStrategyStep(8)) },
					{ "SaveEntity", new BaseStrategyStepInfo(() => new IntegerStrategyStep(16)) },
					{ "Test", new BaseStrategyStepInfo(() => new BaseResultStrategyStepTester(x => result = (int)x)) }
				}, importConfig);

			importStrategy.Execute(5);
			Assert.AreEqual(34, result);
		}
		[Test]
		public void AddStringStrategy()
		{
			var importConfig = ObjectFactory.Get<IStrategyConfigurator>()
				.InitStep("Input",
					stepBuilder => stepBuilder
						.OnSuccessDo("GetOrCreateEntity",
							getOrCreateStep => getOrCreateStep.OnSuccessDo("Mapp",
								mappStep => mappStep.OnSuccessDo("SaveEntity", saveEntityStep => saveEntityStep.OnSuccessDo("Test"))
							)
							.OnErrorDo("Test")
						)
						.OnErrorDo("Test")
				);

			string result = "";
			var importStrategy = ObjectFactory.Get<IStrategyImplementationBuilder>()
				.Build<string>(new Dictionary<string, IStrategyStepInfo>()
				{
					{ "Input", new BaseStrategyStepInfo(() => new StringFormatStrategyStep("S1{0}")) },
					{ "GetOrCreateEntity", new BaseStrategyStepInfo(() => new StringFormatStrategyStep("S2{0}")) },
					{ "Mapp", new BaseStrategyStepInfo(() => new StringFormatStrategyStep("S3{0}")) },
					{ "SaveEntity", new BaseStrategyStepInfo(() => new StringFormatStrategyStep("S4{0}")) },
					{ "Test", new BaseStrategyStepInfo(() => new BaseResultStrategyStepTester(x => result = (string)x)) }
				}, importConfig);

			importStrategy.Execute("Start");
			Assert.AreEqual("S4S3S2S1Start", result);
			importStrategy.Execute("Finish");
			Assert.AreEqual("S1Finish", result);
		}
	}
}
