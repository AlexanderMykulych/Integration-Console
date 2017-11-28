using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class MappRuleOptimizatorFactory: BaseAttributeFactory<IMappRuleOptimizator, MappRuleOptimizatorAttribute, Type>
	{
		private static Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator> _instances;

		protected override Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator> Instances
		{
			get
			{
				if (_instances == null)
				{
					_instances = new Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator>();
				}
				return _instances;
			}
		}

		private static bool _isRuleRegister;

		protected override bool IsRuleRegister
		{
			get { return _isRuleRegister; }
			set { _isRuleRegister = value; }
		}

		protected override bool IsInstanceNameEqual(KeyValuePair<MappRuleOptimizatorAttribute, IMappRuleOptimizator> instanceKeyValue, Type name)
		{
			return instanceKeyValue.Key.RuleType == name;
		}
	}
}
