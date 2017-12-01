using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class CoreDiModule: Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			BindSettings();
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
