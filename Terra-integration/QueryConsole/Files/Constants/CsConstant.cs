using Newtonsoft.Json.Linq;
using QueryConsole.Files.Integrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.CsConfiguration;
using Terrasoft.TsConfiguration;

namespace QueryConsole.Files.Constants
{
	public static class CsConstant
	{
		#region Class: IntegrationResult
		public class IntegrationResult
		{

			#region Properties: Public
			public bool Success { get; set; }
			public JObject Data { get; set; }
			public TResultType Type { get; set; }
			public TResultException Exception { get; set; }
			public string ExceptionMessage { get; set; }
			#endregion

			#region Constructor: Public
			public IntegrationResult()
			{

			}

			public IntegrationResult(JObject data)
			{
				Data = data;
			}

			public IntegrationResult(TResultType type, JObject data = null)
			{
				Type = type;
				Data = data;
			}

			public IntegrationResult(TResultException exception, string message = null, JObject data = null)
			{
				Type = TResultType.Exception;
				Exception = exception;
				ExceptionMessage = message;
				Data = data;
			}
			#endregion

			#region Enum: Public
			public enum TResultException
			{
				OnCreateEntityExist
			}
			public enum TResultType
			{
				Exception,
				Success
			}
			#endregion
		}
		#endregion

		#region Class: IntegrationInfo
		public class IntegrationInfo
		{

			#region Properties: Public
			public JObject Data { get; set; }
			public string StrData { get; set; }
			public UserConnection UserConnection { get; set; }
			public TIntegrationType IntegrationType { get; set; }
			public string EntityName { get; set; }
			public string Action { get; set; }
			public Guid? EntityIdentifier { get; set; }
			public IntegrationResult Result { get; set; }
			public Entity IntegratedEntity { get; set; }
			public string TsExternalIdPath { get; set; }
			public string TsExternalVersionPath { get; set; }
			public EntityHandler Handler {get; set;}
			public Entity ParentEntity { get; set; }
			#endregion

			#region Constructor: Public
			public IntegrationInfo(JObject data, UserConnection userConnection, TIntegrationType integrationType = TIntegrationType.Export,
					Guid? entityIdentifier = null, string entityName = "", string action = "Create", Entity integratedEntity = null)
			{
				Data = data;
				UserConnection = userConnection;
				IntegrationType = integrationType;
				EntityIdentifier = entityIdentifier;
				EntityName = entityName;
				Action = action;
				IntegratedEntity = integratedEntity;
			}
			#endregion

			#region Method Override: public
			public override string ToString()
			{
				return string.Format("Data = {0}\nIntegrationType={1} EntityIdentifier={2}", Data, IntegrationType.ToString(), EntityIdentifier);
			}
			#endregion

			public static IntegrationInfo CreateForImport(UserConnection userConnection, string action, string serviceEntityName, JObject data)
			{
				return new IntegrationInfo(data, userConnection, TIntegrationType.Import, null, serviceEntityName, action, null);
			}
			public static IntegrationInfo CreateForExport(UserConnection userConnection, Entity entity)
			{
				return new IntegrationInfo(null, userConnection, TIntegrationType.Export, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.Empty, entity);
			}
			public static IntegrationInfo CreateForResponse(UserConnection userConnection, Entity entity)
			{
				return new IntegrationInfo(null, userConnection, TIntegrationType.ExportResponseProcess, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.UpdateFromResponse, entity);
			}
		}
		#endregion

		#region Enum: Public
		public enum TIntegrationType
		{
			Export = 0,
			Import = 1,
			All = 3,
			ExportResponseProcess = 4
		}
		public enum TSysAdminUnitType
		{
			Organization = 0,
			Unit = 1,
			Head = 2,
			Team = 3,
			User = 4,
			SelfServicePortalUser = 5,
			FunctionalRole = 6
		}
		#endregion

		#region Properties: Public
		public const string clientserviceEntityUrl = "http://api.client-service.stage2.laximo.ru/v2/entity/AUTO3N";
		public const string clientserviceDictUrl = "http://api.client-service.stage2.laximo.ru/v2/dict/AUTO3N";
		public static Dictionary<string, string> clientserviceEntity = new Dictionary<string, string>() {
			{ "Account", "CompanyProfile" },
			{ "Contact", "PersonProfile" },
			{ "ContactCommunication", "ContactRecord" },
			{ "TsAutomobile", "VehicleProfile" },
			{ "SysAdminUnit", "Manager" },
			{ "SysAdminUnit2", "ManagerGroup" },
			{ "Case", "ClientRequest" },
			{ "Relationship", "Relationship" },
			{ "ContactCareer", "Employee" },
			{ "TsContactNotifications", "NotificationProfile" },
			{ "TsAccountNotification", "NotificationProfile" },
			{ "ContactAddress", "AddressInfo" },
			{ "TsAutoTechService", "VehicleRelationship" },
			{ "TsAutoOwnerHistory", "VehicleRelationship" },
			{ "TsAutoOwnerInfo", "VehicleRelationship" },
			{ "TsAutoTechHistory", "VehicleRelationship" },
			{ "TsLocSalMarket", "Market" }
		};
		public static Dictionary<string, string> clientserviceDict = new Dictionary<string, string>() {
			//Dictionary
			{ "RelationType", "RelationshipType" },
			{ "CommunicationType", "ContactRecordType" },
			{ "AddressType", "AddressType" },
			{ "TsSto", "VehicleRelationshipType" }
			//AssortmentRequestStatus - unrecognize
		};

		public static class VehicleRelationshipType
		{
			public const int Owner = 1;
			public const int Leasing = 2;
			public const int Driver = 3;
			public const int Service = 4;
			public const int Rent = 5;
			public const int Other = 6;
		}

		public const string ContactEntityName = "PersonProfile";
		public const string AccountEntityName = "CompanyProfile";
		public const string RelationshipTypeEntityName = "RelationshipType";
		public const string ManagerEntityName = "Manager";
		public const string RelationshipEntityName = "Relationship";
		public const string ContactRecordEntityName = "ContactRecordType";
		public const string ManagerGroupEntityName = "ManagerGroup";
		public const string AddressTypeEntityName = "AddressType";
		public const string AutomobilePassportEntityName = "VehiclePassport";
		public const string AutomobileRelationshipEntityName = "VehicleRelationship";
		public const string ContactNotificationProfileEntityName = "NotificationProfile";
		public const string AutomobileRelTypeEntityName = "VehicleRelationshipType";

		public static Dictionary<string, string> DefaultBusEventFilters = new Dictionary<string, string>() {
			{"isRead", "false"}
		};
		public static Dictionary<string, string> DefaultBusEventSorts = new Dictionary<string, string>() {
			{"createdAt", "asc"}
		};
		#endregion

		#region Static Class: IntegrationEventName
		public static class IntegrationEventName
		{
			public const string BusEventNotify = @"BusEventNotification";
			public const string BusEventNotifyData = @"BusEventNotificationData";
		}
		#endregion

		#region Static Class: IntegrationActionName
		public static class IntegrationActionName
		{
			public const string Create = @"create";
			public const string Update = @"update";
			public const string Delete = @"delete";
			public const string UpdateFromResponse = @"updateFromResponse";
			public const string Empty = @"";
		}
		#endregion

		#region Static Class: SysSettingsCode
		public static class SysSettingsCode
		{
			public const string AllowImport = @"IntegrServAllowImport";
			public const string IntegrationServiceBaseUrl = @"IntegrServBaseUrl";
			public const string TerrasoftPostboxId = @"IntegrServTerrasoftPostboxId";
			public const string NotificationLimit = @"IntegrServBusEventNotificationLimin";
			public const string IsInsertToDB = @"IntegrServInsertToDbWithoutEntityLogic";
			public const string ClientServiceBaseUrl = @"ClientServiceBaseUrl";
			public const string ConfigurationData = @"IntegrationXmlConfigData";
			public const string IsIntegrationActive = @"IsIntegrationActive";
		}
		#endregion

		#region Static Class: IntegrationFlagSetting
		public static class IntegrationFlagSetting
		{
			public const bool AllowErrorOnColumnAssign = false;
		}
		#endregion

		public static class ServiceColumnInBpm {
			public const string Identifier = @"TsExternalId";
			public const string IdentifierOrder = @"TsOrderServiceId";
			public const string IdentifierManagerInfo = @"TsManagerInfoId";
			public const string Version = @"TsExternalVersion";
			public const string VersionOrder = @"TsOsVersion";
			public const string VersionManagerInfo = @"TsManagerVersion";
		}


		public static class TsRequestType {
			public static readonly Guid Push = new Guid("bda8d5fb-3c8f-41c6-9823-44615ab20596");
			public static readonly Guid GetResponse = new Guid("173dc5c7-0d32-4512-86b8-e91691b22c19");
		}

		public static class PersonName {
			public const string Bpm = @"Bpm";
			public const string ClientService = @"Client-Service";
			public const string IntegrationService = @"Integration-Service";
			public const string OrderService = @"Order-Service";
		}
		public static class TsRequestStatus {
			public static readonly Guid Success = new Guid("5a0d25f5-d718-45ab-b4e3-d615ef7e09c6");
			public static readonly Guid Error = new Guid("88c5e88e-410d-4d67-99c3-722d92f93631");
		}

		public static class TsAddressType {
			public static readonly Guid Delivery = new Guid("760bf68c-4b6e-df11-b988-001d60e938c6");
		}
		public static class TsContractState
		{
			public static readonly Guid Signed = new Guid("1f703f42-f7e8-4e3f-9b54-2b85f62ea507");
		}
		public static class IntegratorSettings {
			public static Dictionary<Type, Dictionary<TServiceObject, string>> Urls = new Dictionary<Type,Dictionary<TServiceObject,string>>() {
				{ typeof(Terrasoft.CsConfiguration.ClientServiceIntegrator), new Dictionary<TServiceObject, string>() {
						//{ TServiceObject.Dict, "http://api.client-service.bus.stage2.auto3n.ru/v2/dict/AUTO3N" },
						{ TServiceObject.Entity, "http://api.client-service.bus.stage2.auto3n.ru/v2/entity/AUTO3N" }
						////{ TServiceObject.Dict, @"http://bus.stage2.auto3n.ru:8080/client-service/v2/dict/AUTO3N" },
						//{ TServiceObject.Entity, "http://api.client-service.stage3.laximo.ru/v2/entity/AUTO3N" }
					}
				},
				{ typeof(OrderServiceIntegrator), new Dictionary<TServiceObject, string>() {
						//{ TServiceObject.Dict, "http://api.order-service.bus2.auto3n.ru/v2/dict/AUTO3N" },
						//{ TServiceObject.Entity, "http://api.order-service.bus2.auto3n.ru/v2/entity/AUTO3N" }
						//{ TServiceObject.Dict, @"http://api.order-service.stage2.laximo.ru//v2/entity/AUTO3N" },
						{ TServiceObject.Entity, @"http://api.order-service.bus.stage2.auto3n.ru/v2/entity/AUTO3N" }
				
					}
				},
			};

			public static Dictionary<Type, string> Names = new Dictionary<Type,string>() {
				{ typeof(Terrasoft.CsConfiguration.ClientServiceIntegrator), "ClientService" },
				{ typeof(OrderServiceIntegrator), "OrderService" },
			};

			public static Dictionary<TServiceObject, string> GetUrlsByServiceName(string serviceName) {
				var serviceType = Names.FirstOrDefault(x => x.Value == serviceName).Key;
				if(Urls.ContainsKey(serviceType)) {
					return Urls[serviceType];
				}
				return new Dictionary<TServiceObject,string>();
			}
		}

		public static class XmlManagerConstant {
			public static readonly string XmlConfigRootNodeName = @"MapingConfiguration";
			public static readonly string XmlConfigEntityConfigNodeName = @"integrationHandlerConfig";
		}
	}
}
