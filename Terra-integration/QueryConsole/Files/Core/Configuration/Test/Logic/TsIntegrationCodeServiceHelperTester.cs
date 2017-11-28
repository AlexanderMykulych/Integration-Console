using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public class TsIntegrationCodeServiceHelperTester
	{
		public static void TestJsonToXml()
		{
			string name, xml;
			var helper = new TsIntegrationCodeServiceHelper(new UserConnection(new AppConnection()));
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/JsonToXmlTest.json", FileMode.Open)))
			{
				xml = helper.GetXmlConfigFromJson(reader.ReadToEnd(), out name);
			}
			Console.WriteLine(name);
			Console.WriteLine(xml);
		}

		public static void TestXmlToJson()
		{
			var helper = new TsIntegrationCodeServiceHelper(new UserConnection(new AppConnection()));
			string json;
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/XmlToJsonTest.txt", FileMode.Open)))
			{
				json = helper.GetJsonConfigFromXml(reader.ReadToEnd(), "some name");
			}
			Console.WriteLine(json);
		}

		public static void TestConfig(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			string result = helper.TestToJson(new TestExportInfo()
			{
				ConfigId = "ContactExport",
				EntityId = "b6aa40ea-7062-4d3a-afa3-000016c0b6df"
			});
			Console.WriteLine(result);
		}
		public static void TestConfigToEntity(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			string json;
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/TestJsonToEntityConfig.json", FileMode.Open)))
			{
				json = reader.ReadToEnd();
			}
			string result = helper.TestToEntity(new TestImportInfo()
			{
				ConfigId = "ContactExport",
				IsExists = true,
				IsUpdate = true,
				Json = json
			});
			Console.WriteLine(result);
		}

		public static void TestGetEntityInfo(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var schemaInfos = helper.GetAllEntityNames();
			schemaInfos.ForEach(x => Console.WriteLine(x.Name + " - " + x.Caption));
		}

		public static void GetBlockLogDataForAnalyze(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var result = helper.GetBlockLogDataForAnalyze(new Guid("7AE02CAA-49CC-4F11-AFEC-D1E604F9D887"));
			Console.WriteLine(result);
		}
		public static void TestServiceByMock(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var info = new TestServiceInfo()
			{
				Id = new Guid("df34de78-81a7-4531-a215-000007e3d012"),
				RouteKey = "Contact",
				SchemaName = "Contact",
				IsUseMock = true
			};
			helper.TestServiceByMock(info);
		}
	}
}