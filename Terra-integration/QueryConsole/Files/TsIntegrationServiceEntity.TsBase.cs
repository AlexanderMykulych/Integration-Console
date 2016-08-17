﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Newtonsoft.Json.Linq;
using Terrasoft.Core.DB;
using System.Data;
using Terrasoft.Common;
using IntegrationInfo = Terrasoft.TsConfiguration.CsConstant.IntegrationInfo;

namespace Terrasoft.TsConfiguration
{
	public abstract class EntityHandler : IIntegrationEntityHandler
	{
		public MappingHelper Mapper;
		public string EntityName;
		public string JName;

		public virtual string HandlerName
		{
			get
			{
				return EntityName;
			}
		}
		public virtual string SettingName
		{
			get
			{
				return JName;
			}
		}
		private string _ServiceName;
		public virtual string ServiceName
		{
			get
			{
				if (string.IsNullOrEmpty(_ServiceName))
				{
					_ServiceName = IntegrationConfigurationManager.IntegrationPathConfig.Paths.FirstOrDefault(x => x.Name == SettingName).ServiceName;
				}
				return _ServiceName;
			}
		}

		public virtual TServiceObject ServiceObjectType
		{
			get
			{
				return TServiceObject.Entity;
			}
		}

		public virtual string Filters
		{
			get
			{
				return null;
			}
		}

		public virtual string ExternalIdPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.Identifier;
			}
		}
		public virtual string ExternalVersionPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.Version;
			}
		}

		public virtual bool IsJsonWithHeader
		{
			get
			{
				return true;
			}
		}

		public virtual void Create(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			var entitySchema = integrationInfo.UserConnection.EntitySchemaManager.GetInstanceByName(EntityName);
			integrationInfo.IntegratedEntity = entitySchema.CreateEntity(integrationInfo.UserConnection);
			integrationInfo.IntegratedEntity.SetDefColumnValues();
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection), IsJsonWithHeader);
			AfterMapping(integrationInfo);
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName);
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Success
				};
				AfterEntitySave(integrationInfo);
			}
			catch (Exception e)
			{
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Exception,
					ExceptionMessage = e.Message
				};
			}
		}
		public virtual void BeforeMapping(IntegrationInfo integrationInfo)
		{
		}

		public virtual void AfterMapping(IntegrationInfo integrationInfo)
		{
		}

		public virtual void AfterEntitySave(IntegrationInfo integrationInfo)
		{
		}

		public virtual void Update(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			var entity = GetEntityByExternalId(integrationInfo);
			if (entity != null)
			{
				integrationInfo.IntegratedEntity = entity;
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
				AfterMapping(integrationInfo);
				Mapper.SaveEntity(entity, JName);
				AfterEntitySave(integrationInfo);
			}
			else
			{
				throw new Exception(string.Format("Can not create entity {0}", EntityName));
			}
		}

		public virtual void Delete(IntegrationInfo integrationInfo)
		{
			//var entity = Mapper.GetEntityByExternalId(EntityName, integrationInfo.Data[JName].Value<int>("id"));
			//entity.SetColumnValue("TsDeleteInIntegrate", true);
			//Mapper.SaveEntity(entity);
		}

		public virtual void Unknown(IntegrationInfo integrationInfo)
		{
			Update(integrationInfo);
		}

		public virtual bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			Mapper.UserConnection = integrationInfo.UserConnection;
			int externalId = 0;
			if (integrationInfo.IntegratedEntity != null)
			{
				externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
			}
			return Mapper.CheckIsExist(EntityName, integrationInfo.Data[JName].Value<int>("id"), integrationInfo.TsExternalIdPath, externalId);
		}

		public virtual void ProcessResponse(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			integrationInfo.Data = Mapper.GetJObject(integrationInfo.StrData);
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
			AfterMapping(integrationInfo);
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, true);
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Success
				};
			}
			catch (Exception e)
			{
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Exception,
					ExceptionMessage = e.Message
				};
			}
		}

		public virtual JObject ToJson(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
			AfterMapping(integrationInfo);
			return integrationInfo.Data;
		}

		public virtual List<MappingItem> GetMapConfig(UserConnection userConnection)
		{
			return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName);
		}

		public virtual Entity GetEntityByExternalId(CsConstant.IntegrationInfo integrationInfo)
		{
			string externalIdPath = integrationInfo.TsExternalIdPath;
			var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
			esq.AddAllSchemaColumns();
			esq.RowCount = 1;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")));
			return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
		}

		public virtual bool IsExport(CsConstant.IntegrationInfo integrationInfo)
		{
			return true;
		}

		public virtual ServiceRequestInfo GetRequestInfo(CsConstant.IntegrationInfo integrationInfo)
		{
			var requestInfo = new ServiceRequestInfo()
			{
				ServiceObjectId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath).ToString(),
				ServiceObjectName = JName,
				Type = ServiceObjectType,
				RequestJson = integrationInfo.Result.Data.ToString(),
				Entity = integrationInfo.IntegratedEntity
			};
			if (IsEntityAlreadyExist(integrationInfo))
			{
				requestInfo.Method = TRequstMethod.PUT;
			}
			else
			{
				requestInfo.Method = TRequstMethod.POST;
			}
			requestInfo.FullUrl = ServiceUrlMaker.MakeUrl(CsConstant.IntegratorSettings.GetUrlsByServiceName(ServiceName)[ServiceObjectType], requestInfo);
			requestInfo.Handler = this;
			return requestInfo;
		}
	}

	[ImportHandlerAttribute("CompanyProfile")]
	[ExportHandlerAttribute("Account")]
	public class AccountHandler : EntityHandler
	{
		public AccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "CompanyProfile";
		}
	}

	[ImportHandlerAttribute("PersonProfile")]
	[ExportHandlerAttribute("Contact")]
	public class ContactHandler : EntityHandler
	{
		public ContactHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "PersonProfile";
		}

		//public override void BeforeMapping(IntegrationInfo integrationInfo)
		//{
		//	if(integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import) {
		//		DeleteAllCommunication
		//	}
		//}
	}

	[ImportHandlerAttribute("VehicleRelationship")]
	[ExportHandlerAttribute("TsAutoOwnerInfo")]
	[ExportHandlerAttribute("TsAutoOwnerHistory")]
	[ExportHandlerAttribute("TsAutoTechService")]
	[ExportHandlerAttribute("TsAutoTechHistory")]
	public class TsAutoOwnerInfoHandler : EntityHandler
	{
		public TsAutoOwnerInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "";
			JName = "VehicleRelationship";
		}
		string handlerName = "TsAutoOwnerInfo";
		public override string HandlerName
		{
			get
			{
				return handlerName;
			}
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				var typeId = integrationInfo.Data[JName]["type"]["#ref"]["id"].Value<int>();
				EntityName = GetEntityNameByTypeId(typeId);
				Mapper.UserConnection = integrationInfo.UserConnection;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public string GetEntityNameByTypeId(int typeId)
		{
			switch (typeId)
			{
				case 1:
				case 2:
					handlerName = "TsAutoOwnerInfo";
					return "TsAutoOwnerInfo";
				case 3:
				case 4:
					handlerName = "TsAutoOwnerHistory";
					return "TsAutoOwnerHistory";
				case 5:
					handlerName = "TsAutoTechHistory";
					return "TsAutoTechHistory";
				case 6:
					handlerName = "TsAutoTechService";
					return "TsAutoTechService";
				default:
					return "TsAutoTechService";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				EntityName = integrationInfo.IntegratedEntity.GetType().Name;
				handlerName = EntityName;
			}
		}
	}

	[ImportHandlerAttribute("Relationship")]
	[ExportHandlerAttribute("Relationship")]
	public class RelationshipHandler : EntityHandler
	{
		public RelationshipHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Relationship";
			JName = "Relationship";
		}
	}

	[ImportHandlerAttribute("NotificationProfileContact")]
	[ExportHandlerAttribute("NotificationProfileContact")]
	public class TsContactNotificationsHandler : EntityHandler
	{
		public TsContactNotificationsHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsContactNotifications";
			JName = "NotificationProfile";
		}
		public override string SettingName
		{
			get
			{
				return "NotificationProfileContact";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsContact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsContact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}

	[ImportHandlerAttribute("NotificationProfileAccount")]
	[ExportHandlerAttribute("NotificationProfileAccount")]
	public class TsAccountNotificationHandler : EntityHandler
	{
		public TsAccountNotificationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAccountNotification";
			JName = "NotificationProfile";
		}
		public override string SettingName
		{
			get
			{
				return "NotificationProfileAccount";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAccount", integrationInfo.Data[JName].Value<string>("parentId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAccount", integrationInfo.Data[JName].Value<string>("parentId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}



	[ImportHandlerAttribute("Manager")]
	[ImportHandlerAttribute("ManagerGroup")]
	[ExportHandlerAttribute("SysAdminUnit")]
	public class SysAdminUnitHandler : EntityHandler
	{
		public ServiceUrlMaker UrlMaker;

		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public SysAdminUnitHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "SysAdminUnit";
			JName = "";
			UrlMaker = new ServiceUrlMaker(CsConstant.IntegratorSettings.Settings[typeof(ClientServiceIntegrator)].BaseUrl);
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			base.BeforeMapping(integrationInfo);
			if (string.IsNullOrEmpty(JName))
			{
				var typeIndex = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue");
				if (typeIndex < 4)
				{
					JName = "ManagerGroup";
				}
				else
				{
					JName = "Manager";
				}
			}
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (JName == "Manager")
				{
					integrationInfo.IntegratedEntity.SetColumnValue("SysAdminUnitTypeValue", CsConstant.TSysAdminUnitType.User);
				}
				else
				{
					integrationInfo.IntegratedEntity.SetColumnValue("SysAdminUnitTypeValue", CsConstant.TSysAdminUnitType.Unit);
				}
			}
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			Mapper.UserConnection = integrationInfo.UserConnection;
			return Mapper.CheckIsExist("SysAdminUnit", integrationInfo.Data[JName].Value<int>("id"), "TsExternalId", integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsExternalId"));
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			if (JName == "Manager" && integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue") == (int)CsConstant.TSysAdminUnitType.User)
			{
				try
				{
					ResaveContact(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("ContactId"), integrationInfo.UserConnection);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "SysAdminUnitHandler.ProcessResponse");
				}
			}
		}

		public void ResaveContact(Guid contactId, UserConnection userConnection)
		{
			if (contactId != Guid.Empty)
			{
				var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Contact");
				esq.AddAllSchemaColumns();
				var entity = esq.GetEntity(userConnection, contactId);
				entity.Save(false);
			}
		}
	}

	[ImportHandlerAttribute("CompanyProfileAssignment")]
	[ExportHandlerAttribute("TsAccountManagerGroup")]
	public class TsAccountManagerGroupHandler : EntityHandler
	{
		public TsAccountManagerGroupHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAccountManagerGroup";
			JName = "CompanyProfileAssignment";
		}
	}

	[ImportHandlerAttribute("ClientRequest")]
	[ExportHandlerAttribute("Case")]
	public class CaseHandler : EntityHandler
	{
		public CaseHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Case";
			JName = "ClientRequest";
		}
	}

	[ImportHandlerAttribute("VehicleProfile")]
	[ExportHandlerAttribute("TsAutomobile")]
	public class TsAutomobileHandler : EntityHandler
	{
		public TsAutomobileHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAutomobile";
			JName = "VehicleProfile";
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			integratePassport(integrationInfo);
		}

		public void integratePassport(IntegrationInfo integrationInfo)
		{
			var automobileId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
			if (automobileId != Guid.Empty)
			{
				var helper = new ClientServiceIntegrator(integrationInfo.UserConnection);
				helper.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new VehiclePassportHandler());
			}
		}
		//public override 
	}

	[ImportHandlerAttribute("VehiclePassport")]
	[ExportHandlerAttribute("VehiclePassport")]
	public class VehiclePassportHandler : EntityHandler
	{
		public VehiclePassportHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAutomobile";
			JName = "VehiclePassport";
		}

		public override string ExternalIdPath
		{
			get
			{
				return "TsPassportExternalId";
			}
		}
		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}

	}

	[ImportHandlerAttribute("ContactInfo")]
	[ExportHandlerAttribute("ContactInfo")]
	public class ContactInfoHandler : EntityHandler
	{
		public ContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "ContactInfo";
		}

		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
	}

	[ImportHandlerAttribute("AddressInfo")]
	[ExportHandlerAttribute("ContactAddress")]
	public class AddressInfoHandler : EntityHandler
	{
		public AddressInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}

	[ImportHandlerAttribute("AddressInfoAccount")]
	[ExportHandlerAttribute("AddressInfoAccount")]
	public class AddressInfoAccountHandler : EntityHandler
	{
		public AddressInfoAccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName
		{
			get
			{
				return "AddressInfoAccount";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("ContactRecord")]
	[ExportHandlerAttribute("ContactCommunication")]
	public class ContactRecordHandler : EntityHandler
	{
		public ContactRecordHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}

	[ImportHandlerAttribute("ContactRecordAccount")]
	[ExportHandlerAttribute("ContactRecordAccount")]
	public class AccountCommunicationHandler : EntityHandler
	{
		public AccountCommunicationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName
		{
			get
			{
				return EntityName;
			}
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("Employee")]
	[ExportHandlerAttribute("ContactCareer")]
	public class ContactCareerHandler : EntityHandler
	{
		public ContactCareerHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactCareer";
			JName = "Employee";
		}
	}

	[ImportHandlerAttribute("Market")]
	[ExportHandlerAttribute("TsLocSalMarket")]
	public class TsLocSalMarketHandler : EntityHandler
	{
		public bool TypeIsLp = false;
		public static readonly Guid TypeLp = new Guid("f11e685a-060d-43cc-a221-26246317257d");
		public TsLocSalMarketHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsLocSalMarket";
			JName = "Market";
		}
		//public override List<MappingItem> GetMapConfig(UserConnection userConnection)
		//{
		//	if (TypeIsLp)
		//	{
		//		return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName, "lp");
		//	}
		//	return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName, "gp");
		//}

		//public override void BeforeMapping(IntegrationInfo integrationInfo)
		//{
		//	if(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsMarketTypeId") == TypeLp) {
		//		TypeIsLp = true;
		//	}
		//}

		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);
			var salesPerCapita = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsSalesForPersonInReg") + integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsSalesForPersonInRegG");
			var capacity = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsMarketVolume") + integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsMarketVolumeG");
			var population = Math.Max(integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsPopulationCount"), integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsPopulationCountG"));
			result[JName]["salesPerCapita"] = JToken.FromObject(salesPerCapita);
			result[JName]["capacity"] = JToken.FromObject(capacity);
			result[JName]["population"] = JToken.FromObject(population);

			//ValidateType(integrationInfo, result);
			return result;
		}
		public void ValidateType(IntegrationInfo integrationInfo, JObject jObj)
		{
			var type = jObj[JName]["type"].ToString();
			switch (type)
			{
				case "cargo":
					jObj[JName]["additionalInfo"]["auto"].Remove();
					break;
				case "auto":
					jObj[JName]["additionalInfo"]["cargo"].Remove();
					break;
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
				{
					var type = integrationInfo.Data[JName]["type"].ToString();
					bool isLp = false;
					bool isGp = false;
					switch (type)
					{
						case "cargo":
							isGp = true;
							break;
						case "auto":
							isLp = true;
							break;
						case "cargo+auto":
						case "auto+cargo":
							isGp = true;
							isLp = true;
							break;
					}
					integrationInfo.IntegratedEntity.SetColumnValue("TsCargoProgram", isGp);
					integrationInfo.IntegratedEntity.SetColumnValue("TsPassengerProgram", isLp);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "AfterEntitySave");
			}
		}
	}

	[ImportHandlerAttribute("Payment")]
	[ExportHandlerAttribute("TsPayment")]
	public class PaymentHandler : EntityHandler
	{
		public PaymentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsPayment";
			JName = "Payment";
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			try
			{
				var orderId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsOrder");
				if (orderId != Guid.Empty)
				{
					UpdatePaymentSum(orderId, integrationInfo.UserConnection);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "PaymentHandler.AfterEntitySave");
			}
		}
		public void UpdatePaymentSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
									.Top(1)
									.Column(Func.Sum("p", "TsAmount")).As("amount")
									.From("TsPaymentInOrder").As("pio")
									.InnerJoin("TsPayment").As("p").On("pio", "TsPaymentId").IsEqual("p", "Id")
									.Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			var update = new Update(userConnection, "Order")
									.Set("PaymentAmount", select)
									.Where("Id").IsEqual(Column.Parameter(orderId)) as Update;
			update.Execute();
		}
	}

	[ImportHandlerAttribute("Order")]
	[ExportHandlerAttribute("Order")]
	public class OrderHandler : EntityHandler
	{
		public OrderHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Order";
			JName = "Order";
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				var name = integrationInfo.Data[JName]["createdByUser"].Value<string>();
				if (name.ToLower() == "shop")
				{
					integrationInfo.Data[JName]["createdByUser"] = "Supervisor";
				}
			}
		}

		public void importTransportationPointCompany(IntegrationInfo integrationInfo)
		{
			try
			{
				var transportationId = integrationInfo.Data.GetJTokenValuePath<int>(JName + ".shipmentInfo.ShipmentInfo.transportationPointId");
				var transportationName = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.transportationPointName");
				var transportationCompanyId = integrationInfo.Data.GetJTokenValuePath<int>(JName + ".shipmentInfo.ShipmentInfo.transportationCompanyId");
				var transportationCompanyName = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.transportationCompanyName");
				var isCurier = isCourierDeliveryMethod(integrationInfo);
				var shipmentPointId = GetTransportationIdAndCreateIfNotExist(transportationId, transportationName, integrationInfo.UserConnection);
				var shipmentCompanyId = GetTransportationCompanyIdAndCreateIfNotExist(transportationCompanyId, transportationCompanyName, integrationInfo.UserConnection);
				if (shipmentPointId != Guid.Empty)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("TsShipmentPointId", shipmentPointId);
				}
				if (shipmentCompanyId != Guid.Empty)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("TsTransportCompanyId", shipmentCompanyId);
				}
				integrationInfo.IntegratedEntity.SetColumnValue("TsIsCourierDelivery", isCurier);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "importTransportationPointCompany");
				throw;
			}
		}
		private Guid GetTransportationIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if(transId == 0 || string.IsNullOrEmpty(transName))
			{
				return Guid.Empty;
			}
			try
			{
				var select = new Select(userConnection)
								.Column("Id").As("Id")
								.From("TsShipmentPoint")
								.Where("TsExternalId").IsEqual(Column.Parameter(transId)) as Select;
				using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(executor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<Guid>("Id");
						}
					}
				}

				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnection)
								.Into("TsShipmentPoint")
								.Set("Id", Column.Parameter(resultId))
								.Set("TsExternalId", Column.Parameter(transId))
								.Set("Name", Column.Parameter(transName));
				insert.Execute();
				return resultId;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "GetTransportationIdAndCreateIfNotExist");
			}
			return Guid.Empty;
		}

		private Guid GetTransportationCompanyIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if(transId == 0 || string.IsNullOrEmpty(transName))
			{
				return Guid.Empty;
			}
			try
			{
				var select = new Select(userConnection)
								.Column("Id").As("Id")
								.From("TsTransportCompany")
								.Where("TsExternalId").IsEqual(Column.Parameter(transId)) as Select;
				using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(executor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<Guid>("Id");
						}
					}
				}

				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnection)
								.Into("TsTransportCompany")
								.Set("Id", Column.Parameter(resultId))
								.Set("TsExternalId", Column.Parameter(transId))
								.Set("Name", Column.Parameter(transName));
				insert.Execute();
				return resultId;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "TsShipmentHandler.GetTransportationCompanyIdAndCreateIfNotExist");
			}
			return Guid.Empty;
		}
		public bool isCourierDeliveryMethod(IntegrationInfo integrationInfo)
		{
			try
			{
				var additionalInfo = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.additionalInfo");
				if (!string.IsNullOrEmpty(additionalInfo))
				{
					var jObject = JObject.Parse(additionalInfo);
					return jObject.GetJTokenValuePath<bool>("deliveryMethod.DeliveryMethod.isCourierDelivery");
				}
				return false;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "TsShipmentHandler.isCourierDeliveryMethod");
			}
			return false;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					if (integrationInfo.Data["Order"]["shipmentInfo"].HasValues)
					{
						var userConnection = integrationInfo.UserConnection;
						var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
						var country = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["country"].Value<string>();
						var region = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["region"].Value<string>();
						var place = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["place"].Value<string>();
						var district = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["district"].Value<string>();
						var street = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["street"].Value<string>();
						var building = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["building"].Value<string>();
						var appartament = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["appartament"].Value<string>();
						var zipCode = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["zipCode"].Value<string>();
						var address = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["address"].Value<string>();
						address = string.Format("{0}, {1}, {2}, {3}", street, building, appartament, address);
						ImportAddress(id, integrationInfo.UserConnection,
								GetGuidByValue("Country", country, userConnection),
								GetGuidByValue("Region", region, userConnection),
								GetGuidByValue("City", place, userConnection),
								GetGuidByValue("TsCounty", district, userConnection),
								IfNullThanEmpty(address),
								IfNullThanEmpty(zipCode),
								CsConstant.TsAddressType.Delivery);
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[1 block]");
				}
				try
				{
					var OrderItemSum = GetOrderItemSum(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
					integrationInfo.IntegratedEntity.SetColumnValue("Amount", OrderItemSum);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[2 block]");
				}
				try
				{
					integrationInfo.IntegratedEntity.SetColumnValue("PaymentAmount", integrationInfo.IntegratedEntity.GetColumnValue("PrimaryPaymentAmount"));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[3 block]");
				}

				try
				{
					importTransportationPointCompany(integrationInfo);
				}
				catch(Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[4 block]");
				}
				try
				{
					if (integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsContractId") != Guid.Empty)
					{
						var account = GetAccountByContract(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsContractId"), integrationInfo.UserConnection);
						if (account != Guid.Empty)
						{
							integrationInfo.IntegratedEntity.SetColumnValue("AccountId", account);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[5 block]");
				}
				try
				{
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[6 block]");
				}
			}
		}

		public string IfNullThanEmpty(string text)
		{
			return string.IsNullOrEmpty(text) ? string.Empty : text;
		}
		public static Guid GetGuidByValue(string schemaName, string value, UserConnection userConnetion, string columnValue = "Name", string primaryColumn = "Id")
		{
			if (string.IsNullOrEmpty(value))
			{
				return Guid.Empty;
			}
			var select = new Select(userConnetion)
						.Column(primaryColumn).As("Id")
						.From(schemaName)
						.Where(columnValue).IsEqual(Column.Parameter(value)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public void ImportAddress(Guid orderId, UserConnection userConnection, Guid country, Guid region, Guid city, Guid tsCountry, string address, string zipCode, Guid addressType)
		{
			var orderAddressId = GetOrderAddres(orderId, userConnection);
			if (orderAddressId == Guid.Empty)
			{
				InsertOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address);
			}
			else
			{
				UpdateOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address);
			}
		}

		public Guid GetOrderAddres(Guid orderId, UserConnection userConnetion)
		{
			var select = new Select(userConnetion)
						.Column("Id")
						.From("TsOrderAddress").As("src")
						.Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public Guid InsertOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address)
		{
			var addressId = Guid.NewGuid();
			var columns = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var insert = new Insert(userConnection)
						.Into("TsOrderAddress")
						.Set("Id", Column.Parameter(addressId))
						.Set("Primary", Column.Parameter(true))
						.Set("Zip", Column.Parameter(zip))
						.Set("Address", Column.Parameter(address)) as Insert;
			foreach (var column in columns)
			{
				if (column.Value != Guid.Empty)
				{
					insert.Set(column.Key, Column.Parameter(column.Value));
				}
			}
			insert.Execute();
			return addressId;
		}
		public void UpdateOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address)
		{
			var columns = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var update = new Update(userConnection, "TsOrderAddress")
						.Set("Primary", Column.Parameter(true))
						.Set("Zip", Column.Parameter(zip))
						.Set("Address", Column.Parameter(address))
						.Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Update;
			foreach (var column in columns)
			{
				if (column.Value != Guid.Empty)
				{
					update.Set(column.Key, Column.Parameter(column.Value));
				}
			}
			update.Execute();
		}

		public Guid GetAccountFromContract(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								.Column("AccountId").As("Id")
								.From("Contract").As("c")
								.Where("Id").IsEqual(Column.Parameter(id)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public double GetOrderItemSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								.Column(Func.Sum("TotalAmount")).As("amount")
								.From("OrderProduct")
								.Where("OrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<double>(reader, "amount");
					}
				}
			}
			return 0;
		}

		public double GetPaymentSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								   .Column(Func.Sum("TsAmount")).As("amount")
								   .From("TsPaymentInOrder").As("pio")
								   .InnerJoin("TsPayment").As("p").On("pio", "TsPaymentId").IsEqual("p", "Id")
								   .Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<double>(reader, "amount");
					}
				}
			}
			return 0;
		}

		public Guid GetAccountByContract(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
							.Column("AccountId")
							.From("Contract")
							.Where("Id").IsEqual(Column.Parameter(id)) as Select;

			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "AccountId");
					}
				}
			}
			return Guid.Empty;
		}
	}

	[ImportHandlerAttribute("OrderItem")]
	[ExportHandlerAttribute("OrderProduct")]
	public class OrderProductHandler : EntityHandler
	{
		public OrderProductHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "OrderProduct";
			JName = "OrderItem";
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					if (integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo") != null)
					{
						var orderId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("OrderId");
						var productId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("ProductId");
						var id = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.id").Value<int>();
						var catalog = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalog").Value<string>();
						var catalogV = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalogVehicleId").Value<int>();
						var ssd = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.ssd").Value<string>();
						var vih = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.vin").Value<string>();
						var frame = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.frame").Value<string>();
						var catCate = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalogCategoryId").Value<int>();
						var unit = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.unitId").Value<int>();
						var clientData = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.clientData").Value<string>();
						var detailCode = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.detailCode").Value<string>();
						if (id != 0)
						{
							var auto = JsonEntityHelper.GetEntityByExternalId("TsAutomobile", int.Parse(catalogV.ToString()), integrationInfo.UserConnection, false, "Id");
							if (JsonEntityHelper.isEntityExist("TsOrderAddInfo", integrationInfo.UserConnection, new Dictionary<string, object>() {
						{ "TsExternalId", id }
					}))
							{
								var update = new Update(integrationInfo.UserConnection, "TsOrderAddInfo")
											.Set("TsCatalog", Column.Parameter(catalog))
											.Set("TsSSD", Column.Parameter(ssd))
											.Set("TsVIN", Column.Parameter(vih))
											.Set("TsOrderId", Column.Parameter(orderId))
											.Set("TsProductId", Column.Parameter(productId))
											.Set("TsFrame", Column.Parameter(frame))
											.Set("TsCategory", Column.Parameter(catCate))
											.Set("TsQuantity", Column.Parameter(unit))
											.Set("TsClientInfo", Column.Parameter(clientData))
											.Set("TsCode", Column.Parameter(detailCode))
											.Where("TsExternalId").IsEqual(Column.Parameter(id)) as Update;
								var autoId = auto.Item2 != null ? auto.Item2.GetTypedColumnValue<Guid>(auto.Item1["Id"]) : Guid.Empty;
								if (autoId != Guid.Empty)
								{
									update.Set("TsAutomobileId", Column.Parameter(autoId));
								}
								update.Execute();
							}
							else
							{
								var insert = new Insert(integrationInfo.UserConnection)
												.Into("TsOrderAddInfo")
												.Set("TsExternalId", Column.Parameter(id))
												.Set("TsCatalog", Column.Parameter(catalog))
												.Set("TsOrderId", Column.Parameter(orderId))
												.Set("TsProductId", Column.Parameter(productId))
												.Set("TsSSD", Column.Parameter(ssd))
												.Set("TsVIN", Column.Parameter(vih))
												.Set("TsFrame", Column.Parameter(frame))
												.Set("TsCategory", Column.Parameter(catCate))
												.Set("TsQuantity", Column.Parameter(unit))
												.Set("TsClientInfo", Column.Parameter(clientData))
												.Set("TsCode", Column.Parameter(detailCode));
								var autoId = auto.Item2 != null ? auto.Item2.GetTypedColumnValue<Guid>(auto.Item1["Id"]) : Guid.Empty;
								if (autoId != Guid.Empty)
								{
									insert.Set("TsAutomobileId", Column.Parameter(autoId));
								}
								insert.Execute();
							}
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[1 block]");
				}

				try
				{
					var totalAmount = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TotalAmount");
					integrationInfo.IntegratedEntity.SetColumnValue("PrimaryTotalAmount", totalAmount);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[2 block]");
				}
			}
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			base.Create(integrationInfo);

			CreateProduct(integrationInfo);
		}


		public override void Update(IntegrationInfo integrationInfo)
		{
			base.Update(integrationInfo);
			CreateProduct(integrationInfo);
		}
		public void CreateProduct(IntegrationInfo integrationInfo)
		{
			var entity = integrationInfo.IntegratedEntity;
			var articul = integrationInfo.Data["OrderItem"]["oem"].Value<string>();
			integrationInfo.IntegratedEntity = GetProductByArticuleOrCreateNew(integrationInfo.UserConnection, articul);
			integrationInfo.IntegratedEntity.SetDefColumnValues();
			Mapper.StartMappByConfig(integrationInfo, JName, IntegrationConfigurationManager.GetConfigItem(integrationInfo.UserConnection, "Product"));
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName);
				var productId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				entity.SetColumnValue("ProductId", productId);
				entity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "OrderProductHandler.CreateProduct");
			}
		}

		public Entity GetProductByArticuleOrCreateNew(UserConnection userConnection, string articule)
		{
			var productId = GetProductIdByArticule(userConnection, articule);
			if (productId == Guid.Empty)
			{
				var schema = userConnection.EntitySchemaManager.GetInstanceByName("Product");
				var entity = schema.CreateEntity(userConnection);
				entity.SetDefColumnValues();
				return entity;
			}
			else
			{
				var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Product");
				esq.AddAllSchemaColumns();
				return esq.GetEntity(userConnection, productId);
			}
		}

		public Guid GetProductIdByArticule(UserConnection userConnection, string articule)
		{
			var select = new Select(userConnection)
						.Column("Id")
						.From("Product")
						.Where("Code").IsEqual(Column.Parameter(articule)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("Return")]
	[ExportHandlerAttribute("TsReturn")]
	public class TsReturnHandler : EntityHandler
	{
		public TsReturnHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsReturn";
			JName = "Return";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("ReturnItem")]
	[ExportHandlerAttribute("TsReturnPosition")]
	public class TsReturnPositionHandler : EntityHandler
	{
		public TsReturnPositionHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsReturnPosition";
			JName = "ReturnItem";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("Shipment")]
	[ExportHandlerAttribute("TsShipment")]
	public class TsShipmentHandler : EntityHandler
	{
		public TsShipmentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsShipment";
			JName = "Shipment";
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			//importTransportationPointCompany(integrationInfo);
		}

		public bool isCourierDeliveryMethod(IntegrationInfo integrationInfo)
		{
			try
			{
				var additionalInfo = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.additionalInfo");
				if (!string.IsNullOrEmpty(additionalInfo))
				{
					var jObject = JObject.Parse(additionalInfo);
					return jObject.GetJTokenValuePath<bool>("deliveryMethod.DeliveryMethod.isCourierDelivery");
				}
				return false;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "TsShipmentHandler.isCourierDeliveryMethod");
			}
			return false;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		
	}

	[ImportHandlerAttribute("ShipmentItem")]
	[ExportHandlerAttribute("TsShipmentPosition")]
	public class TsShipmentPositionHandler : EntityHandler
	{
		public TsShipmentPositionHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsShipmentPosition";
			JName = "ShipmentItem";
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			base.Create(integrationInfo);

			UpdateProduct(integrationInfo);
		}


		public override void Update(IntegrationInfo integrationInfo)
		{
			base.Update(integrationInfo);
			UpdateProduct(integrationInfo);
		}

		public void UpdateProduct(IntegrationInfo integrationInfo)
		{
			var eom = integrationInfo.Data["ShipmentItem"]["oem"].Value<string>();
			var productId = GetProductIdByArticule(integrationInfo.UserConnection, eom);
			if (productId != Guid.Empty)
			{
				var unitName = integrationInfo.Data["ShipmentItem"]["unitName"].Value<string>();
				updateProductUnitName(integrationInfo.UserConnection, productId, unitName);
			}
		}

		public Guid GetProductIdByArticule(UserConnection userConnection, string articule)
		{
			var select = new Select(userConnection)
						.Column("Id")
						.From("Product")
						.Where("Code").IsEqual(Column.Parameter(articule)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public void updateProductUnitName(UserConnection userConnection, Guid productId, string unitName)
		{
			var unitId = OrderHandler.GetGuidByValue("Unit", unitName, userConnection, "ShortName");
			if (unitId == Guid.Empty)
				return;
			var update = new Update(userConnection, "Product")
						.Set("UnitId", Column.Parameter(unitId))
						.Where("Id").IsEqual(Column.Parameter(productId)) as Update;
			update.Execute();
			var updateOrderProduct = new Update(userConnection, "OrderProduct")
						.Set("UnitId", Column.Parameter(unitId))
						.Where("ProductId").IsEqual(Column.Parameter(productId)) as Update;
			updateOrderProduct.Execute();
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("ContractBalance")]
	[ExportHandlerAttribute("Contract")]
	public class ContractBalanceHandler : EntityHandler
	{
		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public ContractBalanceHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contract";
			JName = "ContractBalance";
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			integrationInfo.Data["id"] = integrationInfo.Data["ContractBalance"]["contract"]["#ref"]["id"];
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			integrationInfo.Data["ContractBalance"]["id"] = integrationInfo.Data["ContractBalance"]["contract"]["#ref"]["id"];
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	[ImportHandlerAttribute("Contract")]
	[ExportHandlerAttribute("Contract")]
	public class ContractHandler : EntityHandler
	{
		public ContractHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contract";
			JName = "Contract";
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data == null)
			{
				integrationInfo.Data = new JObject();
			}
			integrationInfo.Data["id"] = integrationInfo.Data["contract.#ref.id"];
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				SetState(integrationInfo);
				SetBussinesProtocol(integrationInfo);
			}
		}
		public void SetState(IntegrationInfo integrationInfo)
		{
			try
			{
				var isActive = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsActive");
				if (isActive)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("StateId", CsConstant.TsContractState.Signed);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "ContractHandler.SetState");
			}
		}
		public void SetBussinesProtocol(IntegrationInfo integrationInfo)
		{
			try
			{
				var accountId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
				if (accountId != Guid.Empty && integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var isLegal = IsAccountLegal(accountId, integrationInfo.UserConnection);
					integrationInfo.IntegratedEntity.SetColumnValue(isLegal ? "TsB2B" : "TsB2C", true);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "ContractHandler.SetBussinesProtocol");
			}
		}

		public bool IsAccountLegal(Guid accountId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
							.Column("TsIsLawPerson").As("IsLegal")
							.From("Account")
							.Where("Id").IsEqual(Column.Parameter(accountId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<bool>(reader, "IsLegal");
					}
				}
			}
			throw new Exception("IsAccountLegal throw exception: No account with id = " + accountId.ToString());
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("StateId") == CsConstant.TsContractState.Signed;
		}
	}

	[ImportHandlerAttribute("Debt")]
	[ExportHandlerAttribute("TsContractDebt")]
	public class TsContractDebtHandler : EntityHandler
	{
		public TsContractDebtHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsContractDebt";
			JName = "Debt";
		}


	}

	[ImportHandlerAttribute("ManagerInfo")]
	[ExportHandlerAttribute("Contact")]
	public class ManagerInfoHandler : EntityHandler
	{
		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public override string ExternalIdPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.IdentifierManagerInfo;
			}
		}

		public override string ExternalVersionPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.VersionManagerInfo;
			}
		}

		public ManagerInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "ManagerInfo";
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return IsContactHaveSysAdminUnit(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.IntegratedEntity.UserConnection);
		}

		public bool IsContactHaveSysAdminUnit(Guid contactId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Column(Func.Count("Id")).As("count")
						.From("SysAdminUnit").As("sau")
						.Where("sau", "ContactId").IsEqual(Column.Parameter(contactId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count") > 0;
					}
				}
			}
			return false;
		}
	}
	[ImportHandlerAttribute("CounteragentContactInfo")]
	[ExportHandlerAttribute("Contact")]
	public class CounteragentContactInfoHandler : EntityHandler
	{
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["CounteragentContactInfo"]["positionFull"] = integrationInfo.Data["CounteragentContactInfo"]["position"];
			}
		}
		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public override string ExternalIdPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}

		public CounteragentContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "CounteragentContactInfo";
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{

			var isClient = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsIsClient");
			if (!isClient)
			{
				var account = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
				return IsAccountHaveOrderServiceId(account, integrationInfo.UserConnection);
			}
			return true;
		}
		public bool IsAccountHaveOrderServiceId(Guid accountId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Column(Func.Count("Id")).As("count")
						.From("Account").As("a")
						.Where("a", "Id").IsEqual(Column.Parameter(accountId))
						.And("a", "TsOrderServiceId").IsNotEqual(Column.Const(0)) as Select;
			using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(executor))
				{
					if (reader.Read())
					{
						return reader.GetColumnValue<int>("count") > 0;
					}
				}
			}
			return false;
		}
	}

	[ImportHandlerAttribute("Counteragent")]
	[ExportHandlerAttribute("Account")]
	public class CounteragentHandler : EntityHandler
	{
		public override string HandlerName
		{
			get
			{
				return JName;
			}
		}
		public override string ExternalIdPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath
		{
			get
			{
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}
		public CounteragentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "Counteragent";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return !isAccountExported(integrationInfo) && isAccountContracted(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
		}

		public bool isAccountContracted(Guid accountId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Column(Func.Count("c", "Id")).As("count")
						.From("Contract").As("c")
						.Where("c", "AccountId").IsEqual(Column.Parameter(accountId))
						.And("c", "TsActive").IsEqual(Column.Parameter(true)) as Select;

			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count") > 0;
					}
				}
			}
			return false;
		}

		public bool isAccountExported(IntegrationInfo integrationInfo)
		{
			return integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsDontIntegrate");
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			SetBussinesProtocol(integrationInfo);
		}
		public void SetBussinesProtocol(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var isLegal = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsIsLawPerson");
					integrationInfo.IntegratedEntity.SetColumnValue(isLegal ? "TsB2B" : "TsB2C", true);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				//TODO: Add Logging
			}
		}
	}

	[ImportHandlerAttribute("")]
	[ExportHandlerAttribute("AccountBillingInfo")]
	public class AccountBillingInfoHandler : EntityHandler
	{
		public AccountBillingInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountBillingInfo";
			JName = "";
		}
	}


	[ImportHandlerAttribute("AccountAnniversary")]
	[ExportHandlerAttribute("AccountAnniversary")]
	public class AccountAnniversaryHandler : EntityHandler
	{
		public override bool IsJsonWithHeader
		{
			get
			{
				return false;
			}
		}
		public AccountAnniversaryHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountAnniversary";
			JName = "";
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			base.ToJson(integrationInfo);
			if (integrationInfo.Data.First != null && integrationInfo.Data.First.First != null)
			{
				integrationInfo.Data = (JObject)integrationInfo.Data.First.First;
				return integrationInfo.Data;
			}
			return null;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["accountId"] = integrationInfo.ParentEntity.GetTypedColumnValue<string>("Id");
			}
		}

	}

	[ImportHandlerAttribute("ContactAnniversary")]
	[ExportHandlerAttribute("ContactAnniversary")]
	public class ContactAnniversaryHandler : EntityHandler
	{
		public override bool IsJsonWithHeader
		{
			get
			{
				return false;
			}
		}
		public ContactAnniversaryHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactAnniversary";
			JName = "";
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			base.ToJson(integrationInfo);
			if (integrationInfo.Data.First != null && integrationInfo.Data.First.First != null)
			{
				integrationInfo.Data = (JObject)integrationInfo.Data.First.First;
				return integrationInfo.Data;
			}
			return null;
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["contactId"] = integrationInfo.ParentEntity.GetTypedColumnValue<string>("Id");
			}
		}
	}
}