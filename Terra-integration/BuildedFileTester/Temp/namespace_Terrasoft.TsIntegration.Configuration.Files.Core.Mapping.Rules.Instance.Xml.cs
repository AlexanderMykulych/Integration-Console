using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Mapping.Rules.Instance.Xml {

	#region Class: SimpleOrDefault
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\SimpleOrDefault.cs
		
	*/
	[RuleAttribute("SimpleOrDefault", TIntegrationObjectType.Xml)]
	public class SimpleOrDefault : IMappRule
	{
		private SimpleMappRule simpleRule;
		public SimpleOrDefault()
		{
			simpleRule = new SimpleMappRule();
		}
		public void Export(RuleExportInfo info)
		{
			simpleRule.Export(info);
			var element = info.json.GetObject() as XElement;
			if (string.IsNullOrEmpty(element.Value))
			{
				info.json.FromObject(info.config.Value);
			}
		}
		public void Import(RuleImportInfo info)
		{
			simpleRule.Import(info);
		}
	}

	#endregion

}