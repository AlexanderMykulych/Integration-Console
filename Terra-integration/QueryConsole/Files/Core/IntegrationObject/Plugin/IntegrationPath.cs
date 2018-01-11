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
	public static class IntegrationPath
	{
		public static string GeneratePath(params string[] steps)
		{
			var iObjectProvider = ObjectFactory.Get<IIntegrationObjectProvider>();
			var objectType = iObjectProvider.GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return GenerateJsonPath(steps);
				case TIntegrationObjectType.Xml:
					return GenerateXPath(steps);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		public static string GenerateValuePath(params string[] steps)
		{
			var iObjectProvider = ObjectFactory.Get<IIntegrationObjectProvider>();
			var objectType = iObjectProvider.GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return GenerateJsonPath(steps);
				case TIntegrationObjectType.Xml:
					return GenerateValueXPath(steps);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		private static string GenerateValueXPath(string[] steps)
		{
			return string.Format("string(//{0}/text())", GenerateXPath(steps));
		}

		public static string GenerateXPath(params string[] steps)
		{
			return steps.Aggregate((x, y) => x + "/" + y);
		}
		
		public static string GenerateJsonPath(params string[] steps)
		{
			return steps.Aggregate((x, y) => x + "." + y);
		}
	}
}