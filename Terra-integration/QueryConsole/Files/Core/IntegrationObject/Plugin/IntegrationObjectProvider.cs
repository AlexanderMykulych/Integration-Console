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
namespace Terrasoft.TsIntegration.Configuration {
	public class IntegrationObjectProvider : IIntegrationObjectProvider
	{
		private readonly ISettingProvider _settingsProvider;

		public IntegrationObjectProvider(ISettingProvider settingsProvider)
		{
			_settingsProvider = settingsProvider;
		}
		public TIntegrationObjectType GetObjectType()
		{
			//TODO:
			return _settingsProvider.SelectGlobalFirstByName<TIntegrationObjectType>("TsIntegrationObjectType");
		}
		public virtual IIntegrationObject Parse(string text)
		{
			var objectType = GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return new IntegrJObject(JObject.Parse(text));
				case TIntegrationObjectType.Xml:
					return new IntegrXObject(XElement.Parse(text));
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		public virtual IIntegrationObject NewInstance(string name = null)
		{
			var objectType = GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return new IntegrJObject();
				case TIntegrationObjectType.Xml:
					return new IntegrXObject(name);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}
		public virtual Stream GetMemoryStream(IIntegrationObject iObject)
		{
			var stream = new MemoryStream();
			var encoding = new UTF8Encoding();
			var encodeData = encoding.GetBytes(iObject.ToString());
			stream.Write(encodeData, 0, encodeData.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
		public virtual string GetContentType(IIntegrationObject iObject)
		{
			if (iObject is IntegrXObject)
			{
				return "text/xml";
			}
			else if (iObject is IntegrJObject)
			{
				return "application/json";
			}
			return "text/plain";
		}
	}
}