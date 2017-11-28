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
	public class IntegrXObject : IIntegrationObject
	{
		private XDocument _document;
		private XElement _data;
		public IntegrXObject(string name)
		{
			_data = new XElement(name);
			_document = new XDocument(_data);
		}
		public IntegrXObject(XElement data)
		{
			_data = data;
			_document = new XDocument(_data);
		}
		//Log key=iObject
		public void FromObject(object obj)
		{
			if (obj == null)
			{
				_data.Value = string.Empty;
				return;
			}
			if (obj is byte[])
			{
				obj = FromBinaryObject((byte[])obj);
			}
			if (obj is ValueType || obj is string)
			{
				_data.Value = obj.ToString();
				return;
			}
			var serializer = new XmlSerializer(obj.GetType());
			var document = new XDocument();
			using (var writer = document.CreateWriter())
			{
				serializer.Serialize(writer, obj);
			}
			_data.SetValue(document.Root);
		}
		//Log key=iObject
		public object FromBinaryObject(byte[] binaryData)
		{
			return Convert.ToBase64String(binaryData);
		}
		public object GetObject()
		{
			return _data;
		}
		//Log key=iObject
		public T GetProperty<T>(string name, T defaultValue = default(T))
		{
			if (_data != null)
			{
				if (string.IsNullOrEmpty(name))
				{
					return CastToTempl<T>(_data.Value);
				}
				return CastToTempl<T>(_document.XPathEvaluate(name));
			}
			return defaultValue;
		}
		//Log key=iObject
		public string GetRootName(string defaultValue = null)
		{
			if (_data != null)
			{
				return _data.Name.LocalName;
			}
			return defaultValue;
		}
		//Log key=iObject
		public IIntegrationObject GetSubObject(string path)
		{
			if (_data != null)
			{
				var xElement = _document.XPathSelectElement(path);
				if (xElement == null)
				{
					_document = new XDocument(_data);
					xElement = _document.XPathSelectElement(path);
				}
				return new IntegrXObject(xElement);
			}
			return null;
		}
		//Log key=iObject
		public IEnumerable<IIntegrationObject> GetSubObjects(string path)
		{
			if (_data != null)
			{
				return _document.XPathSelectElements(path).Select(xElement => new IntegrXObject(xElement)).ToList();
			}
			return null;
		}
		//Log key=iObject
		public void InitObject(string rootName = null)
		{
			if (!string.IsNullOrEmpty(rootName))
			{
				_data.Add(new XElement(rootName));
			}
		}
		//Log key=iObject
		public void SetObject(object obj)
		{
			if (obj is XElement)
			{
				_data = (XElement)obj;
			}
		}
		//Log key=iObject
		public void SetProperty(string name, object obj)
		{
			if (_data != null && !string.IsNullOrEmpty(name))
			{
				XElement token = GetXElementByPath(_data, name);
				XElement resToken = null;
				if (obj is XElement)
				{
					resToken = (XElement)obj;
				}
				else if (obj is IntegrXObject)
				{
					resToken = ((IntegrXObject)obj).GetObject() as XElement;
				}
				else
				{
					token.SetValue(obj);
				}
				if (resToken != null)
				{
					token.ReplaceWith(resToken);
				}
			}
		}
		public static T CastToTempl<T>(object obj)
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}
		//Log key=iObject
		public static XElement GetXElementByPath(XElement xElement, string path)
		{
			var pItems = path.Split('/');
			if (pItems.Any())
			{
				if (xElement.Name.LocalName == pItems[0])
				{
					pItems = pItems.Skip(1).ToArray();
				}
				foreach (var pItem in pItems)
				{
					if (xElement.Element(pItem) == null)
					{
						xElement.Add(new XElement(pItem));
					}
					xElement = xElement.Element(pItem);
				}
				return xElement;
			}
			return xElement;
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