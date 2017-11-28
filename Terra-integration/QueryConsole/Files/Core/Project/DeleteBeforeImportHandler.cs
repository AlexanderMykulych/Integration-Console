using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[IntegrationHandlerAttribute("DeleteBeforeImportHandler")]
	public class DeleteBeforeImportHandler: ArhiveSessionIntegrationHandler
	{
		public DeleteBeforeImportHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		protected override string IdsPath
		{
			get { return GetHandlerConfigValue("Delete_IObjectIdPath", base.IdsPath); }
		}
		protected override string DeleteEntityName
		{
			get { return GetHandlerConfigValue("Delete_EntityName", base.DeleteEntityName); }
		}
		protected override string EntityDeleteIdPath {
			get { return GetHandlerConfigValue("Delete_EntityIdName", base.EntityDeleteIdPath); }
		}
		protected override string DeleteLogCaption
		{
			get { return GetHandlerConfigValue("Delete_LogCaption", base.DeleteLogCaption); }
		}
		protected override string DeleteEntityPrimaryColumnName {
			get { return GetHandlerConfigValue("Delete_EntityPrimaryColumnName", base.DeleteEntityPrimaryColumnName); }
		}
		protected override string MainEntityPrimaryColumnName {
			get { return GetHandlerConfigValue("Delete_MainEntityPrimaryColumnName", base.MainEntityPrimaryColumnName); }
		}
	}
}
