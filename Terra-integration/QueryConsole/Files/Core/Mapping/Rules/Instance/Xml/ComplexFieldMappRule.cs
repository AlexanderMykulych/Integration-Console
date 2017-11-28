using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Terrasoft.TsIntegration.Configuration{
	[RuleAttribute("SimpeJoinedColumn")]
	[RuleAttribute("SimpeJoinedColumn", TIntegrationObjectType.Xml)]
	public class ComplexFieldXmlMappRule : IMappRule
	{
		public ComplexFieldXmlMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newValue = info.json.GetProperty<string>(null);
				if (newValue != null)
				{
					resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
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
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
	}
}