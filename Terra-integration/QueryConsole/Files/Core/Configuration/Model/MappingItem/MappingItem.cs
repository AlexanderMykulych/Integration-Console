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
	public partial class MappingItem
	{

		public string TsSourcePath { get; set; }
		public string JSourceName { get; set; }
		public string JSourcePath { get; set; }

		public string TsDestinationPath { get; set; }
		public string TsDestinationName { get; set; }
		public string TsDestinationResPath { get; set; }
		public string MapType { get; set; }
		public TMapExecuteType MapExecuteType { get; set; }
		public CsConstant.TIntegrationType MapIntegrationType { get; set; }
		public bool EFieldRequier { get; set; }

		public string TsExternalIdPath { get; set; }

		public object ConstValue { get; set; }
		public TConstType ConstType { get; set; }

		public bool IgnoreError { get; set; }
		public bool SaveOnResponse { get; set; }

		public string OrderColumn { get; set; }
		public Common.OrderDirection OrderType { get; set; }

		public string HandlerName { get; set; }

		public bool DeleteBeforeExport { get; set; }
		public string BeforeDeleteMacros { get; set; }

		public string MacrosName { get; set; }

		public string TsExternalSource { get; set; }
		public string TsExternalPath { get; set; }
		public string TsDestinationPathToSource { get; set; }
		public string TsDestinationPathToExternal { get; set; }
		public bool SerializeIfNull { get; set; }
		public bool SerializeIfZero { get; set; }
		public string TsDetailName { get; set; }
		public string TsDetailPath { get; set; }
		public string TsDetailResPath { get; set; }
		public string TsTag { get; set; }
		public string TsDetailTag { get; set; }
		public string OverRuleMacros { get; set; }
		public string Selector { get; set; }

		public bool CreateIfNotExist { get; set; }
		public bool AllowNullToOverMacros { get; set; }
		public bool IsAllowEmptyResult { get; set; }

		public bool LoadDependentEntity { get; set; }
		public string HandlerConfigId { get; set; }
		public string ImportRouteId { get; set; }
		public bool IsArrayItem { get; set; }
		public string TsMappingConfigId { get; set; }
		public string Value { get; set; }
		public bool IsParentMapp { get; set; }
		public MappingItem()
		{

		}
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this).ToString();
		}

	}
}