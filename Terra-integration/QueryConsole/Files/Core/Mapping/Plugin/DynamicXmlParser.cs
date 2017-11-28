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
	public static class DynamicXmlParser
	{
		public static T StartMapXmlToObj<T>(XElement node, Type objType = null, object defObj = null, Func<string, string> prepareValuePredicate = null)
		{
			object resultObj = null;
			objType = objType ?? typeof(T);
			if (defObj == null)
			{
				resultObj = Activator.CreateInstance(objType);
			}
			else
			{
				resultObj = defObj.CloneObject();
			}
			var columnsName = objType.GetProperties().Where(x => x.MemberType == MemberTypes.Property).Select(x => x.Name).ToList();
			foreach (var columnName in columnsName)
			{
				PropertyInfo propertyInfo = objType.GetProperty(columnName);
				var xmlAttr = node.Attribute(columnName);
				if (xmlAttr != null)
				{
					string value = xmlAttr.Value;
					if (!string.IsNullOrEmpty(value))
					{
						if (prepareValuePredicate != null)
						{
							value = prepareValuePredicate(value);
						}
						var propertyType = propertyInfo.PropertyType;
						if (propertyType.IsEnum || propertyType == typeof(int))
						{
							propertyInfo.SetValue(resultObj, int.Parse(value));
						}
						else if (propertyType.IsEnum || propertyType == typeof(ulong))
						{
							propertyInfo.SetValue(resultObj, ulong.Parse(value));
						}
						else if (propertyType == typeof(bool))
						{
							propertyInfo.SetValue(resultObj, value != "0");
						}
						else
						{
							propertyInfo.SetValue(resultObj, value);
						}
					}
				}
			}
			if (resultObj is T)
			{
				return (T)resultObj;
			}
			return default(T);
		}
	}
}