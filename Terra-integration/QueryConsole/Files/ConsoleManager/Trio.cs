using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	public class Trio<T1, T2, T3>
	{
		public Trio(T1 first, T2 second, T3 third)
		{
			First = first;
			Second = second;
			Third = third;
		}
		public T1 First { get; set; }
		public T2 Second { get; set; }
		public T3 Third { get; set; }
	}
}
