using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.IntegrationSystem.Remedy
{
	public class RemedyMockIntegrationConfig: IntegrationConfig
	{
		public Func<ServiceConfig, ServiceConfig> PrepareServiceAction;
		private List<ServiceConfig> _serviceConfig;
		public override List<ServiceConfig> ServiceConfig {
			get
			{
				return _serviceConfig;
			}
			set { _serviceConfig = value.Select(PrepareServiceAction).ToList(); }
		}
	}
}
