using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.Settings.SettingsProvider
{
	[TestFixture]
	public class SettingsProviderTest
	{
		[Test, TestCase("MappingConfig")]
		[TestCase("ExportRouteConfig")]
		[TestCase("ImportRouteConfig")]
		[TestCase("ConfigSetting")]
		[TestCase("DefaultMappingConfig")]
		[TestCase("ServiceConfig")]
		[TestCase("TemplateSetting")]
		[TestCase("TriggerConfig")]
		[TestCase("EndPointConfig")]
		public void SettingsProvider(string settingName)
		{
			var provider = ObjectFactory.Get<ISettingProvider>();
			var setting = ConnectionProvider.DoWith(Setuper.userConnection, () => provider.Get(settingName), null);
			
			Assert.IsNotNull(setting);

			var list = setting.SelectFromList<object>();
			Assert.IsNotNull(list);
			Assert.Greater(list.Count(), 0);
		}
	}
}
