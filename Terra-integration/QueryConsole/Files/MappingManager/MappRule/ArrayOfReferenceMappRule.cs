using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public class ArrayOfReferenceMappRule: BaseMappRule
	{
		public ArrayOfReferenceMappRule()
		{
			_type = "arrayofreference";
		}
		public override void Import(RuleImportInfo info)
		{
			try
			{
				if (info.json != null && info.json is JArray)
				{
					var jArray = (JArray) info.json;
					Action integrateRelatedEntity = () =>
					{
						foreach (JToken jArrayItem in jArray)
						{
							JObject jObj = jArrayItem as JObject;
							var externalId = jObj.SelectToken("#ref.id").Value<int>();
							var type = jObj.SelectToken("#ref.type").Value<string>();
							DependentEntityLoader.LoadDependenEntity(type, externalId, info.userConnection, null,
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
		public override void Export(RuleExportInfo info)
		{
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.JSourceName))
			{
				var srcValue = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
				var jArray = new JArray();
				var resultList = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, srcValue, info.config.TsDestinationResPath);
				foreach (var resultItem in resultList)
				{
					var extId = int.Parse(resultItem.ToString());
					if (extId != 0)
					{
						jArray.Add(JToken.FromObject(CsReference.Create(extId, info.config.JSourceName)));
					}
				}
				info.json = jArray;
			}
			else
			{
				info.json = null;
			}
		}
	}
}
