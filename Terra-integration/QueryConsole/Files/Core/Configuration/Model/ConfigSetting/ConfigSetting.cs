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
	public partial class ConfigSetting
	{
		public string Handler { get; set; }
		public string Url { get; set; }
		public string DefaultMappingConfig { get; set; }
		public string Id { get; set; }
		public string JName { get; set; }
		public string EntityName { get; set; }
		public string ExternalIdPath { get; set; }
		public string JsonIdPath { get; set; }
		public string Auth { get; set; }
		public TIntegrationObjectType IntegrationObjectType { get; set; }
		public string Service { get; set; }
		public string ResponseRoute { get; set; }
		public string ResponseMappingConfig { get; set; }
		public string ResponseJName { get; set; }
		public string TemplateName { get; set; }
		public object AdditionalParameter = null;
		public List<HandlerConfig> HandlerConfigs;
	}
}