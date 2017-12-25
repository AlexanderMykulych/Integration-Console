using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Instance;

namespace Terrasoft.TsIntegration.Configuration
{
	public class CoreDiModule: Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			BindSettings();

			BindStrategy();
		}

		private void BindStrategy()
		{
			Bind<IStrategyConfigurator>().To<BaseStrategyConfigurator>();
			Bind(typeof(IStrategy<>)).To(typeof(BaseStrategy<>));
			Bind<IStepBuilder>().To<BaseStepBuilder>();
			Bind<IStrategyImplementationBuilder>().To<BaseStrategyImplementationBuilder>();
			Bind<IExecutorSubscriber>().To<BaseSubscriber>();
			
		}

		protected virtual void BindSettings()
		{
			Bind<IRepositorySettingsProvider>().To<BaseRepositorySettingProvider>();
			Bind<IXmlProvider>().To<BaseXmlProvider>();
			Bind<ISettingProvider>().To<BaseSettingsProvider>();

			Bind<IConnectionProvider>().To<BpmConnectionProvider>();
		}
	}
}
