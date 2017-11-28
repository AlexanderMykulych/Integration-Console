using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ExceptionProcessStrategyAttribute: Attribute
	{
		public string Name;
		public ExceptionProcessStrategyAttribute(string name)
		{
			Name = name;
		}
	}
}
