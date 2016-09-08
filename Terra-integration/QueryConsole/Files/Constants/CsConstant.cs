using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration {
	public static class CsConstant {
		public class IntegrationResult {
			public bool Success {
				get;
				set;
			}
			public JObject Data {
				get;
				set;
			}
			public TResultType Type {
				get;
				set;
			}
			public TResultException Exception {
				get;
				set;
			}
			public string ExceptionMessage {
				get;
				set;
			}


			public IntegrationResult() {

			}

			public IntegrationResult(JObject data) {
				Data = data;
			}

			public IntegrationResult(TResultType type, JObject data = null) {
				Type = type;
				Data = data;
			}

			public IntegrationResult(TResultException exception, string message = null, JObject data = null) {
				Type = TResultType.Exception;
				Exception = exception;
				ExceptionMessage = message;
				Data = data;
			}


			public enum TResultException {
				OnCreateEntityExist
			}
			public enum TResultType {
				Exception,
				Success
			}

		}


		public class IntegrationInfo {

			public JObject Data {
				get;
				set;
			}
			public string StrData {
				get;
				set;
			}
			public UserConnection UserConnection {
				get;
				set;
			}
			public TIntegrationType IntegrationType {
				get;
				set;
			}
			public string EntityName {
				get;
				set;
			}
			public string Action {
				get;
				set;
			}
			public Guid? EntityIdentifier {
				get;
				set;
			}
			public IntegrationResult Result {
				get;
				set;
			}
			public Entity IntegratedEntity {
				get;
				set;
			}
			public string TsExternalIdPath {
				get;
				set;
			}
			public string TsExternalVersionPath {
				get;
				set;
			}
			public EntityHandler Handler {
				get;
				set;
			}
			public Entity ParentEntity {
				get;
				set;
			}


			public IntegrationInfo(JObject data, UserConnection userConnection, TIntegrationType integrationType = TIntegrationType.Export,
			Guid? entityIdentifier = null, string entityName = "", string action = "Create", Entity integratedEntity = null) {
				Data = data;
				UserConnection = userConnection;
				IntegrationType = integrationType;
				EntityIdentifier = entityIdentifier;
				EntityName = entityName;
				Action = action;
				IntegratedEntity = integratedEntity;
			}


			public override string ToString() {
				return string.Format("Data = {0}\nIntegrationType={1} EntityIdentifier={2}", Data, IntegrationType.ToString(), EntityIdentifier);
			}


			public static IntegrationInfo CreateForImport(UserConnection userConnection, string action, string serviceEntityName, JObject data) {
				return new IntegrationInfo(data, userConnection, TIntegrationType.Import, null, serviceEntityName, action, null);
			}
			public static IntegrationInfo CreateForExport(UserConnection userConnection, Entity entity) {
				return new IntegrationInfo(null, userConnection, TIntegrationType.Export, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.Empty, entity);
			}
			public static IntegrationInfo CreateForResponse(UserConnection userConnection, Entity entity) {
				return new IntegrationInfo(null, userConnection, TIntegrationType.ExportResponseProcess, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.UpdateFromResponse, entity);
			}
		}

		public enum TIntegrationType {
			Export = 0,
			Import = 1,
			All = 3,
			ExportResponseProcess = 4
		}
		public enum TSysAdminUnitType {
			Organization = 0,
			Unit = 1,
			Head = 2,
			Team = 3,
			User = 4,
			SelfServicePortalUser = 5,
			FunctionalRole = 6
		}


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

		public static class VehicleRelationshipType {
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
			{"createdAt", "desc"}
		};


		public static class IntegrationEventName {
			public const string BusEventNotify = @"BusEventNotification";
			public const string BusEventNotifyData = @"BusEventNotificationData";
		}


		public static class IntegrationActionName {
			public const string Create = @"create";
			public const string Update = @"update";
			public const string Delete = @"delete";
			public const string UpdateFromResponse = @"updateFromResponse";
			public const string Empty = @"";
		}


		public static class SysSettingsCode {
			public const string AllowImport = @"IntegrServAllowImport";
			public const string IntegrationServiceBaseUrl = @"IntegrServBaseUrl";
			public const string TerrasoftPostboxId = @"IntegrServTerrasoftPostboxId";
			public const string NotificationLimit = @"IntegrServBusEventNotificationLimin";
			public const string IsInsertToDB = @"IntegrServInsertToDbWithoutEntityLogic";
			public const string ClientServiceBaseUrl = @"ClientServiceBaseUrl";
			public const string ConfigurationData = @"IntegrationXmlConfigData";
			public const string IsIntegrationActive = @"IsIntegrationActive";
		}


		public static class IntegrationFlagSetting {
			public const bool AllowErrorOnColumnAssign = false;
		}


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
			public const string Bpm = @"Bpm`online";
			public const string ClientService = @"Client Service";
			public const string IntegrationService = @"Integration Service";
			public const string OrderService = @"Order Service";
		}
		public static class TsRequestStatus {
			public static readonly Guid Success = new Guid("5a0d25f5-d718-45ab-b4e3-d615ef7e09c6");
			public static readonly Guid Error = new Guid("88c5e88e-410d-4d67-99c3-722d92f93631");
		}

		public static class TsAddressType {
			public static readonly Guid Delivery = new Guid("760bf68c-4b6e-df11-b988-001d60e938c6");
		}
		public static class TsContractState {
			public static readonly Guid Signed = new Guid("1f703f42-f7e8-4e3f-9b54-2b85f62ea507");
		}
		public static class IntegratorSettings {
			public static bool IsIntegrationAsync = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsIntegrationAsync", false);
			public static Dictionary<TServiceObject, string> GetUrlsByServiceName(string serviceName) {
				var serviceType = Settings.FirstOrDefault(x => x.Value.Name == serviceName);
				if ((object)serviceType != null) {
					return serviceType.Value.BaseUrl;
				}
				return new Dictionary<TServiceObject, string>();
			}
			public static Dictionary<Type, IntegratorSetting> Settings = new Dictionary<Type, IntegratorSetting>() {
				{
					typeof(ClientServiceIntegrator),
					new IntegratorSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceName", "ClientService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{ TServiceObject.Entity, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceEntityUrl", @"http://api.client-service.stage3.laximo.ru/v2/entity/AUTO3N") },
							{ TServiceObject.Dict, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceDictUrl", @"http://api.client-service.stage3.laximo.ru/v2/dict/AUTO3N") }
						},
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "ClientServiceIsActive", false),
						IsDebugMode = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "ClientServiceisDebugMode", false)
					}
				},
				{
					typeof(OrderServiceIntegrator),
					new IntegratorSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceName", "OrderService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{ 
					    		TServiceObject.Entity,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceEntityUrl", @"http://api.order-service.bus.stage2.auto3n.ru/v2/entity/AUTO3N") 
							},
							{ 
					    		TServiceObject.Dict,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceDictUrl", @"http://api.order-service.bus.stage2.auto3n.ru/v2/dict/AUTO3N") 
							}
						},
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "OrderServiceIsActive", false),
						IsDebugMode = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "OrderServiceIsDebugMode", false),
						DebugModeInfo = new DebugModeInfo()
						{
							FilePath = @"../../Files/Debug/response.json"
						}
					}
				},
				{
					typeof(IntegrationServiceIntegrator),
					new IntegratorIntegrationServiceSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServiceName", "IntegrationService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{ TServiceObject.Entity, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServicePostUrl", @"http://api.integration.bus.stage2.auto3n.ru/v2/entity") }
						},
						PostboxId = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "IntegrationServicePostId", 10004),
						NotifyLimit = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "IntegrationServicePostLimit", 50),
                        IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IntegrationServiceIsActive", false),
						IsDebugMode = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IntegrationServiceIsDebugMode", false),
					}
				}
			};

			public static string MappingConfiguration = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationXmlConfigData", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			public static string LdapDomainName = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DomainArmtek", "");

			#region Class: Setting
			public class IntegratorSetting {
				public Dictionary<TServiceObject, string> BaseUrl;
				public string Name;
				public string Auth;
				public bool IsIntegratorActive;
				public bool IsDebugMode;
				public DebugModeInfo DebugModeInfo;
			}
			public class IntegratorIntegrationServiceSetting: IntegratorSetting {
				public int PostboxId;
				public int NotifyLimit;
			}

			public class DebugModeInfo {
				public string FilePath;

				public string GetDebugDataJson() {
					using (var stream = new StreamReader(new FileStream(FilePath, FileMode.Open))) {
						return stream.ReadToEnd();
					}
				}
			}
			#endregion
		}
		private static AppConnection appConnection;
		private static AppConnection AppConnection {
			get {
				if (appConnection == null) {
					if(HttpContext.Current != null && HttpContext.Current.Application["AppConnection"] != null) {
						appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
					} else {
						appConnection = new AppConnection();
						Initialize("Default");
					}
				}
				return appConnection;
			}
		}
		private static UserConnection _userConnection;
		public static UserConnection UserConnection {
			get {
				if (_userConnection == null) {
					if(HttpContext.Current != null) {
						_userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					}
					if (_userConnection == null) {
						var systemUserConnection = AppConnection.SystemUserConnection;
						//var autoAuthorization = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(
						//	systemUserConnection, "ClientSiteIntegrationAutoAuthorization", false);
						var autoAuthorization = false;
						if (autoAuthorization) {
							string userName = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationUserName");
							if (!string.IsNullOrEmpty(userName)) {
								string userPassword = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationUserPassword");
								string workspace = AppConnection.SystemUserConnection.Workspace.Name;
								_userConnection = new UserConnection(AppConnection);
								_userConnection.Initialize();
								try {
									_userConnection.Login(userName, userPassword, workspace, TimeZoneInfo.Utc);
								} catch (Exception) {
									_userConnection = null;
								}
							}
						} else {
							_userConnection = systemUserConnection;
						}
					}
				}
				if (_userConnection == null) {
					throw new ArgumentException("Invalid login or password");
				}
				return _userConnection;
			}
		}
		private static void Initialize(string workspaceName) {
			AppConfigurationSectionGroup appConfigurationSectionGroup = GetAppSettings();
			var resources = (Terrasoft.Common.ResourceConfigurationSectionGroup)appConfigurationSectionGroup.SectionGroups["resources"];
			GeneralResourceStorage.Initialize(resources);
			var appSettings = (AppConfigurationSectionGroup)appConfigurationSectionGroup;
			string appDirectory = Path.GetDirectoryName(typeof(IntegratorSettings).Assembly.Location);
			appSettings.Initialize(appDirectory, Path.Combine(appDirectory, "App_Data"), Path.Combine(appDirectory, "Resources"),
				appDirectory);
			AppConnection.Initialize(appSettings);
			AppConnection.InitializeWorkspace(workspaceName);
		}
		private static AppConfigurationSectionGroup GetAppSettings() {
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = (AppConfigurationSectionGroup)configuration.SectionGroups["terrasoft"];
			appSettings.RootConfiguration = configuration;
			return appSettings;
		}
		public static class XmlManagerConstant {
			public static readonly string XmlConfigRootNodeName = @"MapingConfiguration";
			public static readonly string XmlConfigEntityConfigNodeName = @"integrationHandlerConfig";
		}
	}
}
