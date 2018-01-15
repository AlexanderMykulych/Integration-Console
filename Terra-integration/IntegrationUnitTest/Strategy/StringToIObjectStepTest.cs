using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.Strategy
{
	[TestFixture]
	[Category("Strategy")]
	public class StringToIObjectStepTest
	{
		private readonly string _json;

		public StringToIObjectStepTest()
		{
			_json = @"{
				""a"": 1,
				""b"": ""text"",
				""c"": {
					""a"": 2,
					""b"": ""text2""
				}
			}";
		}

		[Test]
		[TestCase(TIntegrationObjectType.Json)]
		[TestCase(TIntegrationObjectType.All)]
		[TestCase(TIntegrationObjectType.Excell)]
		[TestCase(TIntegrationObjectType.TsEntity)]
		[TestCase(TIntegrationObjectType.Xml)]
		public void CreateInstance_WithoutError(TIntegrationObjectType type)
		{
			var obj = new StringToIObjectStep(type);
		}

		[Test]
		public void Execute_JsonType_CreateIObject()
		{
			IIntegrationObject result = null;
			var step = new StringToIObjectStep(TIntegrationObjectType.Json);
			step.On("Success", iObject => result = (IIntegrationObject)iObject);
			step.Execute(_json);
			Assert.IsNotNull(result);
		}

		[Test]
		public void Execute_JsonType_CorrectAttributeValues()
		{
			IIntegrationObject result = null;
			var step = new StringToIObjectStep(TIntegrationObjectType.Json);
			step.On("Success", iObject => result = (IIntegrationObject)iObject);
			step.Execute(_json);
			Assert.AreEqual(1, result.GetProperty<int>("a"));
			Assert.AreEqual("text", result.GetProperty<string>("b"));
			Assert.AreEqual(2, result.GetProperty<int>("c.a"));
			Assert.AreEqual("text2", result.GetProperty<string>("c.b"));
		}
	}
}
