using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[MappRuleOptimizator(typeof(SimpleMappRule))]
	public class MappRuleOptimizator : IMappRuleOptimizator
	{
		public void Optimize(CsConstant.IntegrationInfo integrationInfo)
		{
			//throw new NotImplementedException();
		}
	}
}
