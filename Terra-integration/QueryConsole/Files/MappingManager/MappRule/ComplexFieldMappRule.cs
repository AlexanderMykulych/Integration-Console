using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public class ComplexFieldMappRule: BaseMappRule
	{
		public ComplexFieldMappRule()
		{
			_type = "firstdestinationfield";
		}
		public override void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newValue = JsonEntityHelper.GetSimpleTypeValue(info.json);
				if (newValue != null && (info.json.Type != JTokenType.String || newValue.ToString() != ""))
				{
					resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
					if (info.config.CreateIfNotExist && (resultId == null || (resultId is string && (string)resultId == string.Empty) || (resultId is Guid && (Guid)resultId == Guid.Empty)))
					{
						Dictionary<string, string> defaultColumn = null;
						if(!string.IsNullOrEmpty(info.config.TsTag)) {
							defaultColumn = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
							foreach(var columnKey in defaultColumn.Keys.ToList()) {
								string value = defaultColumn[columnKey];
								if (value.StartsWith("$")) {
									defaultColumn[columnKey] = GetAdvancedSelectTokenValue(info.json, value.Substring(1));
								}
							}
						}
						resultId = JsonEntityHelper.CreateColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1, "CreateOn", Common.OrderDirection.Descending, defaultColumn).FirstOrDefault();
					}
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public override void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json = resultObject != null ? JToken.FromObject(resultObject) : null;
		}
		public string GetAdvancedSelectTokenValue(JToken jToken, string path) {
			if (path.StartsWith(".-") && jToken.Parent != null) {
				return GetAdvancedSelectTokenValue(jToken.Parent, path.Substring(2));
			}
			if (jToken != null) {
				var resultToken = jToken.SelectToken(path);
				if(resultToken != null) {
					return resultToken.Value<string>();
				}
			}
			return string.Empty;
		}
	}
}
