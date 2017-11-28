using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terrasoft.Core.Entities;
using Terrasoft.Core;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Handler.Instance {

	#region Class: NoEntityHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\NoEntityHandler.cs
		
	*/
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

	#endregion

}