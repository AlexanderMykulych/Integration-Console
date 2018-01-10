using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace IntegrationUnitTest.Setup
{
	[TestFixture]
	public class SetuperTest
	{
		[Test]
		public void UserConnectionCreateSucces()
		{
			Assert.IsNotNull(Setuper.userConnection);
		}
	}
}
