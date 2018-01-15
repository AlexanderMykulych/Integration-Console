using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using IntegrationUnitTest.BenchMarkNUnitIntegration;
using NUnit.Framework;

namespace IntegrationUnitTest.IntegrationSystem.Remedy
{
	//[TestFixture]
	public class BenchMark
	{
		//[Test]
		public void Test()
		{
			var result = BenchmarkRunner.Run<RemedyServiceTest>(new AllowNonOptimized());

		}
	}
}
