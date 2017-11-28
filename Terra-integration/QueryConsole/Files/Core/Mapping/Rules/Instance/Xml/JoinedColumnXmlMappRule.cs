using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Terrasoft.TsIntegration.Configuration
{
	[RuleAttribute("JoinedColumn", TIntegrationObjectType.Xml)]
	public class JoinedColumnXmlMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newJValue = info.json.GetObject() as XElement;
				if (newJValue != null)
				{
					var newValue = newJValue.Value;
					if (newValue != null)
					{
						resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
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
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
	}
}
