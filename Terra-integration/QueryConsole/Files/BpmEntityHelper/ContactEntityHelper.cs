using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Configuration;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Process;

namespace Terrasoft.TsConfiguration
{
	public static class ContactEntityHelper
	{
		public static ContactSgm GetContactSgm(string name, UserConnection userConnection)
		{
			var converter = GetContactConverter(userConnection);
			return converter.GetContactSgm(name);
		}

		public static IContactFieldConverter GetContactConverter(UserConnection userConnection)
		{
			object converterIdValue = Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "ContactFieldConverter");
			if (converterIdValue == null || string.IsNullOrEmpty(converterIdValue.ToString()))
			{
				return null;
			}
			Guid converterId = Guid.Parse(converterIdValue.ToString());
			if (converterId == Guid.Empty)
			{
				return null;
			}
			var showNamesByESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "ShowNamesBy");
			showNamesByESQ.PrimaryQueryColumn.IsAlwaysSelect = true;
			string convertеrColumnName = showNamesByESQ.AddColumn("Converter").Name;
			string separatorColumnName = showNamesByESQ.AddColumn("Separator").Name;
			showNamesByESQ.Filters.Add(showNamesByESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", converterId));
			EntityCollection showNamesByEntityCollection = showNamesByESQ.GetEntityCollection(userConnection);
			if (showNamesByEntityCollection.Count < 1)
			{
				return null;
			}
			string converterName = showNamesByEntityCollection[0].GetTypedColumnValue<string>(convertеrColumnName);
			if (string.IsNullOrEmpty(converterName))
			{
				return null;
			}
			string separator = showNamesByEntityCollection[0].GetTypedColumnValue<string>(separatorColumnName);
			if (!userConnection.Workspace.IsWorkspaceAssemblyInitialized)
			{
				return null;
			}
			var converter = userConnection.Workspace.WorkspaceAssembly
				.CreateInstance(converterName) as IContactFieldConverter;
			if (converter == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(separator))
			{
				converter.Separator = separator.ToCharArray();
			}
			return converter;
		}

		public static void CreateContactByAccount(Entity accountEntity, UserConnection userConnection,
			bool withCommunication = true, bool withAddress = true, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var contactSchema = userConnection.EntitySchemaManager.GetInstanceByName("Contact");
				var contactEntity = contactSchema.CreateEntity(userConnection);
				contactEntity.SetDefColumnValues();
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				SetContactFieldByAccount(contactEntity, accountEntity, userConnection, false, withCommunication, withAddress, OnErrorAction);
				ReintegrateContactAndAccount(userConnection, contactEntity, accountEntity, OnErrorAction);
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		private static void ReintegrateContactAndAccount(UserConnection userConnection, Entity contactEntity, Entity accountEntity, Action<Exception> OnErrorAction)
		{
			try
			{
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var integrator = new ClientServiceIntegrator(userConnection);
				//http://tscore-task/browse/SKT-4678
				integrator.IntegrateBpmEntity(id, "Account", new AccountHandler());

				integrator.IntegrateBpmEntity(contactId, "Contact");
				accountEntity.SetColumnValue("PrimaryContactId", contactId);
				accountEntity.UpdateInDB(false);
				//KOSTYL!
				IntegrationLocker.Unlock("Account", accountEntity.GetTypedColumnValue<int>("TsOrderServiceId"),
					"OrderService_Counteragent");
				//KOSTYL!
				integrator.IntegrateBpmEntity(id, "Account", new CounteragentHandler());
				var careerId = CreateContactCareerByAccount(userConnection, contactId, id, OnErrorAction);
				if (careerId != Guid.Empty)
				{
					integrator.IntegrateBpmEntity(careerId, "ContactCareer");
				}
			} catch(Exception e)
			{
				if(OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		public static void SetContactFieldByAccount(Entity contactEntity, Entity accountEntity, UserConnection userConnection, bool updateEntity, bool withCommunication, bool withAddress, Action<Exception> OnErrorAction, bool changeAccount = true)
		{
			try
			{
				var name = accountEntity.GetTypedColumnValue<string>("Name");
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var owner = accountEntity.GetTypedColumnValue<Guid>("OwnerId");
				var lrsId = accountEntity.GetTypedColumnValue<Guid>("TsLrsAccountId");
				var spId = accountEntity.GetTypedColumnValue<Guid>("TsSpAccountId");
				var sgm = ContactEntityHelper.GetContactSgm(name, userConnection);
				contactEntity.SetColumnValue("GivenName", sgm.GivenName);
				contactEntity.SetColumnValue("Surname", sgm.Surname);
				contactEntity.SetColumnValue("MiddleName", sgm.MiddleName);
				contactEntity.SetColumnValue("Name", name);
				contactEntity.SetColumnValue("TsBpB2C", true);
				//http://tscore-task/browse/SKT-3728
				contactEntity.SetColumnWithEmptyCheck("TsLrsAccountId", lrsId);
				contactEntity.SetColumnWithEmptyCheck("TsSpAccountId", spId);
				contactEntity.SetColumnWithEmptyCheck("TypeId", CsConstant.EntityConst.ContactType.Client);
				contactEntity.SetColumnWithEmptyCheck("OwnerId", owner);
				if (changeAccount)
				{
					contactEntity.SetColumnWithEmptyCheck("AccountId", id);
				}
				if (updateEntity)
				{
					contactEntity.UpdateInDB(false);
				}
				else
				{
					contactEntity.InsertToDB(false);
				}
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				if (withCommunication)
				{
					CreateContactCommunicationByAccount(id, contactId, userConnection, OnErrorAction);
				}
				if (withAddress)
				{
					CreateContactAddressByAccount(id, contactId, userConnection, OnErrorAction);
				}
			} catch(Exception e)
			{
				if(OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		public static void CreateContactCommunicationByAccount(Guid accountId, Guid contactId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_MigrateAccountCommunicationToContact")
					.WithParameter(Column.Parameter(accountId))
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
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

		public static void CreateContactAddressByAccount(Guid accountId, Guid contactId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_MigrateAccountAddressToContact")
					.WithParameter(Column.Parameter(accountId))
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
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

		public static List<Guid> GetAccountContacts(Guid accountId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var result = new List<Guid>();
				var select = new Select(userConnection)
					.Column("Id")
					.From("Contact")
					.Where("AccountId").IsEqual(Column.Parameter(accountId)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						while (reader.Read())
						{
							result.Add(reader.GetColumnValue<Guid>("Id"));
						}
					}
				}
				return result;
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
				return null;
			}
		}

		public static void ClearAllAddressField(this Entity contactEntity)
		{
			try
			{
				contactEntity.SetColumnValue("AddressTypeId", null);
				contactEntity.SetColumnValue("Address", "");
				contactEntity.SetColumnValue("CityId", null);
				contactEntity.SetColumnValue("RegionId", null);
				contactEntity.SetColumnValue("CountryId", null);
				contactEntity.SetColumnValue("Zip", "");
				contactEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				var addressEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "ContactAddress");
				addressEsq.RowCount = 1;
				addressEsq.AddAllSchemaColumns();
				var orderColumn = addressEsq.AddColumn("CreatedOn");
				orderColumn.OrderByDesc();
				addressEsq.Filters.Add(addressEsq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact",
					contactEntity.GetTypedColumnValue<Guid>("Id")));
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

		public static void ResaveContactPrimaryAddress(UserConnection userConnection, Entity contactEntity, Guid contactId,
			Action<Exception> onException = null)
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
					.From("ContactAddress")
					.Where("ContactId").IsEqual(Column.Parameter(contactId))
					.And("Primary").IsEqual(Column.Const(1)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							contactEntity.SetColumnValue("Address", reader.GetColumnValue<string>("Address"));
							contactEntity.SetColumnValue("CountryId", reader.GetColumnValue<string>("CountryId"));
							contactEntity.SetColumnValue("CityId", reader.GetColumnValue<string>("CityId"));
							contactEntity.SetColumnValue("RegionId", reader.GetColumnValue<string>("RegionId"));
							contactEntity.SetColumnValue("Zip", reader.GetColumnValue<string>("Zip"));
							contactEntity.UpdateInDB(false);
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

		public static void ClearContactPrimaryCommunication(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onException = null)
		{
			try
			{
				contactEntity.SetColumnValue("Phone", string.Empty);
				contactEntity.SetColumnValue("MobilePhone", string.Empty);
				contactEntity.SetColumnValue("HomePhone", string.Empty);
				contactEntity.SetColumnValue("Email", string.Empty);
				contactEntity.SetColumnValue("Skype", string.Empty);
				contactEntity.SetColumnValue("Facebook", string.Empty);
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

		public static void ResaveContactBirthday(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				var select = new Select(userConnection)
					.Top(1)
					.Column("Id")
					.Column("Date")
					.From("ContactAnniversary")
					.Where("ContactId").IsEqual(Column.Parameter(contactEntity.GetTypedColumnValue<Guid>("Id")))
					.And("AnniversaryTypeId").IsEqual(Column.Parameter(CsConstant.EntityConst.AnniversaryType.BirthDate))
					.OrderByDesc("CreatedOn") as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						while (reader.Read())
						{
							var birthDate = reader.GetColumnValue<DateTime>("Date");
							contactEntity.SetColumnValue("BirthDate", birthDate);
							contactEntity.UpdateInDB(false);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
		}
		/// <summary>
		/// http://tscore-task/browse/SKT-4138
		/// http://tscore-task/browse/SKT-4707
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="contactId"></param>
		/// <param name="accountId"></param>
		/// <param name="onErrorAction"></param>
		/// <returns></returns>
		public static Guid CreateContactCareerByAccount(UserConnection userConnection, Guid contactId, Guid accountId,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				if (contactId == Guid.Empty || accountId == Guid.Empty)
				{
					return Guid.Empty;
				}
				var createCareerIfNeedSp = new StoredProcedure(userConnection,
								"tsp_IntegrationHandler_AppendContactCareer")
							.WithParameter("contactId", contactId)
							.WithParameter("accountId", accountId)
							.WithOutputParameter("resultId", userConnection.DataValueTypeManager.GetInstanceByName("Guid"))
						as StoredProcedure;
				createCareerIfNeedSp.PackageName = userConnection.DBEngine.SystemPackageName;
				createCareerIfNeedSp.Execute();
				return (Guid)createCareerIfNeedSp.Parameters.GetByName("resultId").Value;
			}
			catch (Exception e)
			{
				if (onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return Guid.Empty;
		}

		public static bool FindContactByAccount(Entity integratedEntity, UserConnection userConnection, Action<StoredProcedure> storedProcAction, Action<Exception> onErrorAction)
		{
			try
			{
				var advancedSearchInfo = new AdvancedSearchInfo()
				{
					StoredProcedureName = CsConstant.EntityConst.ContactConst.ContactSearchStoredProcedure
				};
				var contactId = advancedSearchInfo.Search(userConnection, storedProcAction, onErrorAction);
				if(contactId == Guid.Empty)
				{
					return false;
				}
				var id = integratedEntity.GetTypedColumnValue<Guid>("Id");
				var contactEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Contact");
				contactEsq.AddAllSchemaColumns(true);
				var contactEntity = contactEsq.GetEntity(userConnection, contactId);
				var isUser = IsContactHasUser(userConnection, contactId);
				SetContactFieldByAccount(contactEntity, integratedEntity, userConnection, true, true, true, onErrorAction, !isUser);
				ReintegrateContactAndAccount(userConnection, contactEntity, integratedEntity, onErrorAction);
				return true;
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return false;
		}
		public static void SynchronizeCommunication(UserConnection userConnection, Guid contactId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_Integration_SynchronizeContactCommunication")
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
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

		public static bool IsContactHasUser(UserConnection userConnection, Guid contactId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				int count = 0;
				var select = new Select(userConnection)
							.Top(1)
							.Column("Id")
							.From("SysAdminUnit")
							.Where("SysAdminUnitTypeValue").IsEqual(Column.Const(CsConstant.TSysAdminUnitType.User))
							.And("ContactId").IsEqual(Column.Parameter(contactId)) as Select;
				select.ExecuteReader(dbReader =>
				{
					if(dbReader.GetColumnValue<Guid>("Id") != Guid.Empty)
					{
						count = 1;
					}
				});
				return count > 0;
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
			return false;
		}
	}
}
