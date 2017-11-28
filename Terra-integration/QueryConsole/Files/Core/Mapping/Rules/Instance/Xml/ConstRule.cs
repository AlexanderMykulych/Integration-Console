using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[RuleAttribute("Const", TIntegrationObjectType.Xml)]
	public class ConstRule : IMappRule
	{
		public void Export(RuleExportInfo info)
		{
			object value = info.config.Value;
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, (string)value);
			}
			info.json.FromObject(value);
		}

		public void Import(RuleImportInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
