using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Validators;

namespace IntegrationUnitTest.BenchMarkNUnitIntegration
{
	public class AllowNonOptimized : ManualConfig
	{
		public AllowNonOptimized()
		{
			Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

			Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
			Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
			Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
			//Add(CsvMeasurementsExporter.Default);
			//Add(RPlotExporter.Default);
			//Set(new DefaultOrderProvider(SummaryOrderPolicy.FastestToSlowest));

			Add(
				new Job("MySuperJob")
				{
					Env = { Runtime = Runtime.Clr },
					Run = { LaunchCount = 1, IterationTime = TimeInterval.Millisecond * 200 }
				});

		}
	}
}
