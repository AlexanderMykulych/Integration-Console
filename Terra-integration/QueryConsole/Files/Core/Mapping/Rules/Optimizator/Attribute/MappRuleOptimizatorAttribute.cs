using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MappRuleOptimizatorAttribute: System.Attribute
	{
		public Type RuleType;
		public MappRuleOptimizatorAttribute(Type ruleType)
		{
			RuleType = ruleType;
		}
	}
}
