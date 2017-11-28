using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUtilsClassLibrary
{
	public class StatisticReverseProjectBuilder: ReverseProjectBuilder
	{
		public StatisticReverseProjectBuilder(string inputFile, string outputFolder) : base(inputFile, outputFolder)
		{
		}

		public override void Run()
		{
			base.Run();
		}
	}
}
