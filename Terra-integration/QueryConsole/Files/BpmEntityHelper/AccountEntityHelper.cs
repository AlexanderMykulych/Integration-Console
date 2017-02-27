using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
	public static class AccountEntityHelper
	{
		public static void ImportInOrderServiceIfNeed(Guid id, UserConnection userConnection)
		{
			try
			{
				var logInfo = LoggerInfo.GetBpmRequestLogInfo(userConnection, CsConstant.PersonName.OrderService, "Account",
					"Counteragent", string.Format("Id = '{{0}}'", id));
				LoggerHelper.DoInTransaction(logInfo, () =>
				{
					var select = new Select(userConnection)
							.Top(1)
							.Column("a", "Id")
							.From("Account").As("a")
							.Where("a", "Id").IsEqual(Column.Parameter(id))
							.And("a", "TsOrderServiceId").IsEqual(Column.Const(0)) as Select;
					using (var dbExecutor = select.UserConnection.EnsureDBConnection())
					{
						using (var reader = select.ExecuteReader(dbExecutor))
						{
							if (reader.Read())
							{
								var integrator = new OrderServiceIntegrator(userConnection);
								integrator.IntegrateBpmEntity(id, "Account", new CounteragentHandler());
							}
						}
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public static Guid GetLrsByAccountId(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Top(1)
						.Column("a", "TsSalesMarketId")
						.From("Account").As("a")
						.InnerJoin("Account").As("aLrs")
						.On("aLrs", "Id").IsEqual("a", "TsLrsAccountId")
						.Where("a", "Id").IsEqual(Column.Parameter(id)) as Select;
			return select.ExecuteScalar<Guid>();
		}
		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity accountEntity, Action<Exception> onErrorAction = null, Action<EntitySchemaQuery> addFilterAction = null)
		{
			try
			{
				var addressEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "AccountAddress");
				addressEsq.RowCount = 1;
				addressEsq.AddAllSchemaColumns();
				var orderColumn = addressEsq.AddColumn("CreatedOn");
				orderColumn.OrderByDesc();
				addressEsq.Filters.Add(addressEsq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account", accountEntity.GetTypedColumnValue<Guid>("Id")));
				if (addFilterAction != null)
				{
					addFilterAction(addressEsq);
				}
				var address = addressEsq.GetEntityCollection(userConnection);
				if (address != null && address.Any())
				{
					AddressHelper.UpdateAddressFromDeliveryService(userConnection, address.FirstOrDefault(), onErrorAction, true);
				}
			}
			catch (Exception e)
			{
				onErrorAction(e);
			}
		}
		public static void ResaveAccountPrimaryAddress(UserConnection userConnection, Entity accountEntity, Guid accountId, Action<Exception> onException = null)
		{
			try
			{
				var select = new Select(userConnection)
								.Top(1)
								.Column("Address")
								.Column("CountryId")
								.Column("CityId")
								.Column("RegionId")
								.Column("Zip")
								.From("AccountAddress")
								.Where("AccountId").IsEqual(Column.Parameter(accountId))
								.And("Primary").IsEqual(Column.Const(1)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							accountEntity.SetColumnValue("Address", reader.GetColumnValue<string>("Address"));
							accountEntity.SetColumnValue("CountryId", reader.GetColumnValue<string>("CountryId"));
							accountEntity.SetColumnValue("CityId", reader.GetColumnValue<string>("CityId"));
							accountEntity.SetColumnValue("RegionId", reader.GetColumnValue<string>("RegionId"));
							accountEntity.SetColumnValue("Zip", reader.GetColumnValue<string>("Zip"));
							accountEntity.UpdateInDB(false);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
		}

		public static void ClearAccountPrimaryCommunication(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onException = null)
		{
			try
			{
				contactEntity.SetColumnValue("Phone", string.Empty);
				contactEntity.SetColumnValue("AdditionalPhone", string.Empty);
				contactEntity.SetColumnValue("Web", string.Empty);
				contactEntity.SetColumnValue("Fax", string.Empty);
				contactEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
		}
		public static void SynchronizeCommunication(UserConnection userConnection, Guid accountId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_Integration_SynchronizeAccountCommunication")
					.WithParameter(Column.Parameter(accountId)) as StoredProcedure;
				storedProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				storedProcedure.Execute();
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
	}
}
