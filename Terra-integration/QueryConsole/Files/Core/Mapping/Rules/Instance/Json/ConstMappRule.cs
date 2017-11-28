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
	[RuleAttribute("Const")]
	public class ConstMappRule : IMappRule
	{
		public ConstMappRule()
		{ }
		public void Import(RuleImportInfo info)
		{
			//throw new NotImplementedException();
		}
		public void Export(RuleExportInfo info)
		{
			object resultValue = null;
			switch (info.config.ConstType)
			{
				case TConstType.String:
					resultValue = info.config.ConstValue;
					break;
				case TConstType.Bool:
					resultValue = Convert.ToBoolean(info.config.ConstValue.ToString());
					break;
				case TConstType.Int:
					resultValue = int.Parse(info.config.ConstValue.ToString());
					break;
				case TConstType.Null:
					resultValue = null;
					break;
				case TConstType.EmptyArray:
					resultValue = new ArrayList();
					break;
			}
			//info.json = resultValue != null ? JToken.FromObject(resultValue) : null;
		}
	}
}