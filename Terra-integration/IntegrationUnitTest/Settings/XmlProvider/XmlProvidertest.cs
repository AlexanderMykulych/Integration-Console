using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.Settings.XmlProvider
{
	[TestFixture]
	public class XmlProviderTest
	{
		[Test]
		public void MergeEmptyXmlTest()
		{
			var provider = ObjectFactory.Get<IXmlProvider>();

			var result = provider.MergeXmls(new List<string>());
			Assert.IsNull(result);
		}
		[Test]
		public void MergeNullXmlTest()
		{
			var provider = ObjectFactory.Get<IXmlProvider>();

			var result = provider.MergeXmls(null);
			Assert.IsNull(result);
		}
	}
}
