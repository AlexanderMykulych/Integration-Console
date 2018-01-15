using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Instance;

namespace Terrasoft.TsIntegration.Configuration
{
	public class CoreDiModule: Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			BindSettings();

			BindStrategy();

			BindIntegrator();
		}

		private void BindIntegrator()
		{
			Bind<IEntityPreparer>().To<EntityPreparer>();
			Bind<IIntegrationObjectWorker>().To<IntegrationObjectWorker>();
			Bind<IServiceHandlerWorkers>().To<ServiceHandlerWorker>();
			Bind<IServiceRequestWorker>().To<ServiceRequestWorker>();
			Bind<IIntegrator>().To<SyncIntegrator>();
			Bind<ISyncExportChecker<Guid>>().To<SyncValidator>();
			Bind<BaseIntegratorMock>().ToSelf();
			Bind<IHandlerEntityWorker>().To<HandlerEntityWorker>();
			Bind<IHandlerKeyGenerator>().To<HandlerKeyGenerator>();
			Bind<IIntegrationObjectProvider>().To<IntegrationObjectProvider>();
			Bind<ITemplateFactory>().To<TemplateHandlerFactory>();
			Bind<IIntegrationService>().To<BaseIntegrationService>();
			Bind<IMapper>().To<IntegrationMapper>();
		}

		private void BindStrategy()
		{
			Bind<IStrategyConfigurator>().To<BaseStrategyConfigurator>();
			Bind(typeof(IStrategy<>)).To(typeof(BaseStrategy<>));
			Bind<IStepBuilder>().To<BaseStepBuilder>();
			Bind<IStrategyImplementationBuilder>().To<BaseStrategyImplementationBuilder>();
			Bind<IExecutorSubscriber>().To<BaseSubscriber>();
			Bind<IMapperDbWorker>().To<MapperDbWorker>();
			Bind<IRuleFactory>().To<RulesFactory>().InSingletonScope();
			Bind<ITriggerEngine<Entity>>().To<TriggerEngine>();
		}

		protected virtual void BindSettings()
		{
			Bind<IRepositorySettingsProvider>().To<BaseRepositorySettingProvider>();
			Bind<IXmlProvider>().To<BaseXmlProvider>();
			Bind<ISettingProvider>().To<BaseSettingsProvider>().InSingletonScope();
			Bind<IIntegrationConfig>().To<IntegrationConfig>();
			Bind<IConfigManager>().To<XmlConfigManager>();
			Bind<IConnectionProvider>().To<BpmConnectionProvider>();
		}
	}
}
