using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public class PrimaryExportProvider
	{
		private BaseServiceIntegrator _integrator;
		public UserConnection UserConnection;
		public PrimaryExportProvider(UserConnection userConnection)
		{
			UserConnection = userConnection;
			_integrator = new OrderServiceIntegrator(UserConnection);
		}

		public PrimaryExportProvider Run(PrimaryExportParam param)
		{
			try
			{
				var entitySelect = new Select(UserConnection)
					.Column("EntitySrc", "Id")
					.From(param.EntityName).As("EntitySrc") as Select;
				if (param.OnlyNew)
				{
					entitySelect.Where("EntitySrc", param.ExternalIdName).IsEqual(Column.Const(0));
				}
				if (param.FilterAction != null)
				{
					param.FilterAction(entitySelect);
				}
				using (var dbExecutor = UserConnection.EnsureDBConnection())
				{
					dbExecutor.ExecuteSelectWithPaging(entitySelect, 0, param.RateCount, "[EntitySrc].[CreatedOn]", reader =>
					{
						while (reader.Read())
						{
							var id = reader.GetColumnValue<Guid>("Id");
							_integrator.IntegrateBpmEntity(id, param.EntityName, param.EntityHandler);
						}
					}, IntegrationLogger.SimpleLoggerErrorAction);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return this;
		}
	}
}
