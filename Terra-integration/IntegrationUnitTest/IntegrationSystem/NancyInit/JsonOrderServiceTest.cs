using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using NUnit.Framework;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.IntegrationSystem
{
	[TestFixture]
	public class JsonOrderServiceTest
	{
		[Test]
		public void SendOnPing_ResultPong()
		{
			var service = ObjectFactory.Get<IIntegrationService>();
			var request = service.Create(new ServiceConfig()
			{
				Method = "GET",
				Url = "http://localhost:1234/ping"
			}, null);
			var responseText = "";
			service.SendRequest(request, response =>
			{
				responseText = service.GetContentFromResponse(response);
			}, null);

			Assert.AreEqual("pong", responseText);
		}

		[Test]
		public void SendOnSum_ResultCorrectSum()
		{
			var service = ObjectFactory.Get<IIntegrationService>();
			var request = service.Create(new ServiceConfig()
			{
				Method = "GET",
				Url = "http://localhost:1234/sum/12-3-46-5"
			}, null);
			var responseText = "";
			service.SendRequest(request, response =>
			{
				responseText = service.GetContentFromResponse(response);
			}, null);

			Assert.AreEqual("66", responseText);
		}
	}

	public class OrderJsonIntegrationService : NancyModule
	{
		public OrderJsonIntegrationService()
		{
			Get["/ping"] = p => "pong";
			Get["/sum/{numberStr}"] = p =>
			{
				var numberStr = (string)p.numberStr;
				return numberStr.Split('-')
					.Select(x => int.Parse(x))
					.Sum()
					.ToString();
			};
		}
	}
}
