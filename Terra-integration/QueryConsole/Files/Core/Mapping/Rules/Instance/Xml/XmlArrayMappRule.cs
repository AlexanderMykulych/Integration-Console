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
	[RuleAttribute("Array", TIntegrationObjectType.Xml)]
	public class XmlArrayMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object value = info.json.GetProperty<object>(null);
			if (value is XElement)
			{
				value = ((XElement)value).Value;
			}
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultImport(info.config.MacrosName, value);
			}
			if (value is ValueType || value is string)
			{
				info.entity.SetColumnValue(info.config.TsSourcePath, value);
			}
		}
		public void Export(RuleExportInfo info)
		{
		}


	}
}