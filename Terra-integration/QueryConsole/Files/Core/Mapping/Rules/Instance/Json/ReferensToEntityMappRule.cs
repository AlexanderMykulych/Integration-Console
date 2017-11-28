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
	[RuleAttribute("RefToGuid")]
	public class ReferensToEntityMappRule : IMappRule
	{
		public ReferensToEntityMappRule()
		{
		}

		public void Import(RuleImportInfo info)
		{
			//Guid? resultGuid = null;
			//if (info.json != null && info.json.HasValues)
			//{
			//	var refColumns = info.json[JsonEntityHelper.RefName];
			//	var externalId = int.Parse(refColumns["id"].ToString());
			//	var type = refColumns["type"].Value<string>();
			//	Func<Guid?> resultGuidAction = () => JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsExternalIdPath,
			//			externalId, info.config.TsDestinationPath, -1, "CreatedOn", Terrasoft.Common.OrderDirection.Descending,
			//			JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',')).FirstOrDefault() as Guid?;
			//	if (info.config.LoadDependentEntity)
			//	{
			//		DependentEntityLoader.LoadDependenEntity(type, externalId, info.userConnection, () =>
			//		{
			//			resultGuid = resultGuidAction();
			//		}, IntegrationLogger.SimpleLoggerErrorAction);
			//	}
			//	else
			//	{
			//		resultGuid = resultGuidAction();
			//	}
			//}
			//if (!info.config.IsAllowEmptyResult && (resultGuid == null || resultGuid.Value == Guid.Empty))
			//{
			//	return;
			//}
			//info.entity.SetColumnValue(info.config.TsSourcePath, resultGuid);
		}

		public void Export(RuleExportInfo info)
		{
			object resultObj = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath,
				info.config.JSourceName, info.config.TsDestinationPath))
			{
				var resultValue =
					JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath,
							info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath), info.config.TsExternalIdPath)
						.FirstOrDefault(x => (int)x > 0);
				if (resultValue != null)
				{
					var resultRef = CsReference.Create(int.Parse(resultValue.ToString()), info.config.JSourceName);
					resultObj = resultRef != null ? JToken.FromObject(resultRef) : null;
				}
			}
			//info.json = resultObj as JToken;
		}
	}
}