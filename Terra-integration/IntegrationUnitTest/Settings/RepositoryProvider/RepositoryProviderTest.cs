using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.Settings
{
	[TestFixture]
	[Category("Settings")]
	public class RepositoryProviderTest
	{
		[Test]
		public void GetXmlTest()
		{
			var settings = ObjectFactory.Get<IRepositorySettingsProvider>();
			var result = ConnectionProvider.DoWith(Setuper.userConnection, () => settings.GetXmls(), null);

			Assert.IsNotNull(result);
			Assert.Greater(result.Count, 0);
		}
	}
}
