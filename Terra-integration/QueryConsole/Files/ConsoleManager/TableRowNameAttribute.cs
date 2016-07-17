using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	[AttributeUsage(AttributeTargets.Property)]
	public class TableRowNameAttribute : Attribute
	{
		public TableRowNameAttribute(string name)
		{
			Name = name;
		}
		public string Name;
	}
}
