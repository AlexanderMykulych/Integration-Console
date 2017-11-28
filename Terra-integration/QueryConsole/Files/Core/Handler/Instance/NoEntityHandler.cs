using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration
{
	[IntegrationHandlerAttribute("NoEntityHandler")]
	public class NoEntityHandler: DefaultEntityHandler
	{
		public NoEntityHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		public override Entity CreateEntityForExportMyMapping(ref MappingConfig mappingConfig, UserConnection userConnection)
		{
			return null;
		}
	}
}
