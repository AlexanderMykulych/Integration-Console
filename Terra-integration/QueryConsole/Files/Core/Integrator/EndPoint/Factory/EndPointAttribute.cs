using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EndPointAttribute : Attribute
	{
		public string Name;
		public EndPointAttribute(string name)
		{
			Name = name;
		}
	}
}
