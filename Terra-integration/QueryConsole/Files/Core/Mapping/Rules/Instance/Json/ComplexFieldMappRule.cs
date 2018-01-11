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
	[RuleAttribute("JoinedColumn")]
	public class ComplexFieldMappRule : IMappRule
	{
		public ComplexFieldMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newJValue = info.json.GetObject() as JValue;
				if (newJValue != null)
				{
					var newValue = newJValue.Value;
					if (newValue != null)
					{
						resultId = JsonEntityHelper.GetColumnValues(info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
						if (info.config.CreateIfNotExist && (resultId == null || (resultId is string && (string)resultId == string.Empty) || (resultId is Guid && (Guid)resultId == Guid.Empty)))
						{
							Dictionary<string, string> defaultColumn = null;
							if (!string.IsNullOrEmpty(info.config.TsTag))
							{
								defaultColumn = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
								foreach (var columnKey in defaultColumn.Keys.ToList())
								{
									string value = defaultColumn[columnKey];
									if (value.StartsWith("$"))
									{
										defaultColumn[columnKey] = GetAdvancedSelectTokenValue(newJValue, value.Substring(1));
									}
								}
							}
							resultId = JsonEntityHelper.CreateColumnValues(info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1, "CreateOn", Common.OrderDirection.Descending, defaultColumn).FirstOrDefault();
						}
					}
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
		public string GetAdvancedSelectTokenValue(JToken jToken, string path)
		{
			if (path.StartsWith(".-") && jToken.Parent != null)
			{
				return GetAdvancedSelectTokenValue(jToken.Parent, path.Substring(2));
			}
			if (jToken != null)
			{
				var resultToken = jToken.SelectToken(path);
				if (resultToken != null)
				{
					return resultToken.Value<string>();
				}
			}
			return string.Empty;
		}
	}
}