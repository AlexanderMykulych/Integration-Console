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
	[RuleAttribute("ArrayOfReference")]
	public class ArrayOfReferenceMappRule : IMappRule
	{
		public ArrayOfReferenceMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			try
			{
				if (info.json != null && info.json is JArray)
				{
					var jArray = (JArray)info.json;
					Action integrateRelatedEntity = () =>
					{
						foreach (JToken jArrayItem in jArray)
						{
							JObject jObj = jArrayItem as JObject;
							var externalId = jObj.SelectToken("#ref.id").Value<int>();
							var type = jObj.SelectToken("#ref.type").Value<string>();
							var userConnection = ObjectFactory
								.Get<IConnectionProvider>()
								.Get<UserConnection>();
							DependentEntityLoader.LoadDependenEntity(type, externalId, userConnection, null,
								IntegrationLogger.SimpleLoggerErrorAction);
						}
					};
					info.AfterEntitySave = integrateRelatedEntity;
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public void Export(RuleExportInfo info)
		{
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.JSourceName))
			{
				//var srcValue = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
				//var jArray = new JArray();
				//var resultList = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, srcValue, info.config.TsDestinationResPath);
				//foreach (var resultItem in resultList)
				//{
				//	var extId = int.Parse(resultItem.ToString());
				//	if (extId != 0)
				//	{
				//		jArray.Add(JToken.FromObject(CsReference.Create(extId, info.config.JSourceName)));
				//	}
				//}
				//info.json = jArray;
			}
			else
			{
				info.json = null;
			}
		}
	}
}