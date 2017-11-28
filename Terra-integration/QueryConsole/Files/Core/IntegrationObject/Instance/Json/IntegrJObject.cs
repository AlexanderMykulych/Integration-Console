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
	public class IntegrJObject : IIntegrationObject
	{
		private JToken _data;
		public IntegrJObject()
		{
			_data = new JObject();
		}
		public IntegrJObject(JToken jObj)
		{
			if (jObj != null)
			{
				SetObject(jObj);
			}
			else
			{
				_data = new JObject();
			}
		}

		public void FromObject(object obj)
		{
			if (obj == null)
			{
				_data = null;
			}
			else
			{
				_data = JToken.FromObject(obj);
			}
		}
		public object GetObject()
		{
			return _data;
		}
		public T GetProperty<T>(string name, T defaultValue = default(T))
		{
			if (_data != null)
			{
				if (string.IsNullOrEmpty(name))
				{
					return _data.Value<T>();
				}
				return _data.SelectToken(name).Value<T>();
			}
			return defaultValue;
		}

		public string GetRootName(string defaultValue = null)
		{
			if (_data != null && _data is JObject)
			{
				return ((JObject)_data).Properties().First().Name;
			}
			return defaultValue;
		}

		public IIntegrationObject GetSubObject(string path)
		{
			if (_data != null)
			{
				var jObj = _data.SelectToken(path);
				return new IntegrJObject(jObj);
			}
			return null;
		}
		public IEnumerable<IIntegrationObject> GetSubObjects(string path)
		{
			return new List<IIntegrationObject>();
		}
		public void InitObject(string rootName = null)
		{
			if (!string.IsNullOrEmpty(rootName))
			{
				_data[rootName] = new JObject();
			}
		}

		public void SetObject(object jObj)
		{
			if (jObj is JToken)
			{
				_data = (JToken)jObj;
			}
		}
		public void SetProperty(string name, object obj)
		{
			if (_data != null && !string.IsNullOrEmpty(name))
			{
				JToken token = GetJTokenByPath(_data, name);
				if (obj == null)
				{
					token.Replace(null);
				}
				JToken resToken = null;
				if (obj is JToken || obj is JObject || obj is JValue)
				{
					resToken = (JToken)obj;
				}
				else if (obj is IntegrJObject)
				{
					var objToken = ((IntegrJObject)obj).GetObject();
					if (objToken == null)
					{
						resToken = null;
					}
					else
					{
						resToken = objToken as JToken;
					}
				}
				token.Replace(resToken);
			}
		}
		public static JToken GetJTokenByPath(JToken jToken, string path)
		{
			var pItems = path.Split('.');
			foreach (var pItem in pItems)
			{
				if (!jToken.HasValues || jToken[pItem] == null)
				{
					jToken[pItem] = new JObject();
				}
				jToken = jToken[pItem];
			}
			return jToken;
		}
		public override string ToString()
		{
			if (_data != null)
			{
				return _data.ToString();
			}
			return string.Empty;
		}
	}
}