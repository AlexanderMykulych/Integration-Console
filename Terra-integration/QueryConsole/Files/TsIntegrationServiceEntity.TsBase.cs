using System;
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
	public abstract class EntityHandler
	{
		public MappingHelper Mapper;
		public string EntityName;
		public string JName;
		public virtual string HandlerName {
			get {
				return EntityName;
			}
		}
		public virtual string SettingName {
			get {
				return JName;
			}
		}
		private string _ServiceName;
		public virtual string ServiceName {
			get {
				if (string.IsNullOrEmpty(_ServiceName))
				{
					var handlerSetting = IntegrationConfigurationManager.IntegrationPathConfig.Paths.FirstOrDefault(x => x.Name == SettingName);
					if (handlerSetting != null)
					{
						_ServiceName = handlerSetting.ServiceName;
					}
					else
					{
						IntegrationLogger.CurrentLogger.Instance.Error(string.Format("Problem with ({0}-{1}-{2}-{3}) handler setting", JName, EntityName, HandlerName, this.GetType().Name));
					}
				}
				return _ServiceName;
			}
		}
		public virtual TServiceObject ServiceObjectType {
			get {
				return TServiceObject.Entity;
			}
		}
		public virtual string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.Identifier;
			}
		}
		public virtual string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.Version;
			}
		}
		public virtual bool IsJsonWithHeader {
			get {
				return true;
			}
		}
		public virtual bool IsEmbeddedObject {
			get {
				return false;
			}
		}

		public virtual string ParentObjectJName {
			get {
				return string.Empty;
			}
		}
		public virtual string ParentObjectTsName {
			get {
				return string.Empty;
			}
		}
		/// <summary>
		/// Признак разширеного поиска
		/// </summary>
		public virtual bool IsAdvancedSearch {
			get {
				return false;
			}
		}
		/// <summary>
		/// Информация о розширеном поиске
		/// </summary>
		public virtual AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return null;
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
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName);
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
				if (IsVersionHigger(integrationInfo))
				{
					BeforeMapping(integrationInfo);
					Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
					AfterMapping(integrationInfo);
					Mapper.SaveEntity(entity, JName, ServiceName);
					AfterEntitySave(integrationInfo);
				}
			}
			else
			{
				throw new Exception(string.Format("Can not create entity {0}", EntityName));
			}
		}
		public virtual void Delete(IntegrationInfo integrationInfo)
		{
			throw new NotImplementedException();
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
			bool result = Mapper.CheckIsExist(EntityName, integrationInfo.Data[JName].Value<int>("id"), integrationInfo.TsExternalIdPath, externalId);
			if (!result && IsAdvancedSearch)
			{
				result = IsEntityAlreadyExistAdvanced(integrationInfo);
			}
			return result;
		}
		public virtual bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			if(integrationInfo.IntegratedEntity != null)
			{
				return true;
			}
			if(AdvancedSearchInfo == null)
			{
				return false;
			}
			Guid resultId = AdvancedSearchInfo.Search(integrationInfo.UserConnection,
					procedure => AddParameterToSearchProcedure(integrationInfo, procedure), IntegrationLogger.SimpleLoggerErrorAction);
			if(resultId == Guid.Empty)
			{
				return false;
			}
			var entityEsq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
			entityEsq.AddAllSchemaColumns();
			integrationInfo.IntegratedEntity = entityEsq.GetEntity(integrationInfo.UserConnection, resultId);
			return true;
		}
		public virtual void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			return;
		}
		public virtual void ProcessResponse(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			if (!string.IsNullOrEmpty(integrationInfo.StrData))
			{
				integrationInfo.Data = Mapper.GetJObject(integrationInfo.StrData);
			}
			if (integrationInfo.IntegratedEntity == null)
			{
				integrationInfo.IntegratedEntity = GetEntityByExternalId(integrationInfo);
			}
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
			AfterMapping(integrationInfo);
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName, true);
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Success
				};
				CheckDuplicateByExternalId(integrationInfo);
			}
			catch (Exception e)
			{
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Exception,
					ExceptionMessage = e.Message
				};
				IntegrationLogger.Error(e, integrationInfo.ToString());
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
			if (IsAdvancedSearch && integrationInfo.IntegratedEntity != null)
			{
				return integrationInfo.IntegratedEntity;
			}
			else
			{
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")));
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
		}
		public virtual bool IsExport(CsConstant.IntegrationInfo integrationInfo)
		{
			return true;
		}
		public virtual ServiceRequestInfo GetRequestInfo(CsConstant.IntegrationInfo integrationInfo)
		{
			var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
			var requestInfo = new ServiceRequestInfo()
			{
				ServiceObjectId = id.ToString(),
				ServiceObjectName = JName,
				Type = ServiceObjectType,
				RequestJson = integrationInfo.Result.Data.ToString(),
				Entity = integrationInfo.IntegratedEntity
			};
			if (id > 0)
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
		public virtual void CheckDuplicateByExternalId(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create && integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess && integrationInfo.IntegratedEntity != null)
				{
					int externalId = GetExternalIdValue(integrationInfo);
					var primaryColumnName = integrationInfo.IntegratedEntity.Schema.GetPrimaryColumnName();
					if (externalId == 0)
					{
						return;
					}
					if (EntityHelper.IsExistDuplicateByExternalId(integrationInfo.UserConnection, EntityName, ExternalIdPath, externalId, IntegrationLogger.SimpleLoggerErrorAction))
					{
						EntityHelper.ClearDuplicateExternalIdByIds(
							integrationInfo.UserConnection,
							EntityName,
							primaryColumnName,
							ExternalIdPath,
							externalId,
							integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>(primaryColumnName),
							IntegrationLogger.SimpleLoggerErrorAction
						);
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public virtual int GetExternalIdValue(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data != null)
			{
				var idToken = integrationInfo.Data.SelectToken(JName + ".id");
				if (idToken != null)
				{
					return idToken.Value<int>();
				}
			}
			return 0;
		}
		public virtual bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			int serviceVersion = 0, bpmVersion = 0;
			if (integrationInfo.IntegratedEntity != null &&
				integrationInfo.IntegratedEntity.IsColumnValueLoaded(ExternalVersionPath))
			{
				bpmVersion = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalVersionPath);
			}
			if (integrationInfo.Data != null)
			{
				serviceVersion = integrationInfo.Data.SelectToken(JName + ".version").Value<int>();
			}
			return serviceVersion > bpmVersion;
		}
	}

	[ImportHandlerAttribute("CompanyProfile")]
	[ExportHandlerAttribute("Account")]
	public class AccountHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;
		

		public AccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "CompanyProfile";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c
			};
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);

			try
			{
				if (!result.IsJTokenPathHasValue("CompanyProfile.taxRegistrationNumber"))
				{
					result.RemoveByPath("CompanyProfile.taxRegistrationNumberName");
					result.RemoveByPath("CompanyProfile.taxRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}

			try
			{
				if (!result.IsJTokenPathHasValue("CompanyProfile.companyRegistrationNumber"))
				{
					result.RemoveByPath("CompanyProfile.companyRegistrationNumberName");
					result.RemoveByPath("CompanyProfile.companyRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return result;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
				{
					integrationInfo.IntegratedEntity.ClearAllAddressField();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			//AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			AccountEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			if (integrationInfo.Data != null)
			{
				if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c)
				{
					var emailPhones = GetEmailPhones(integrationInfo.Data);
					searchProcedure
						.WithParameter("Emails", emailPhones.Item1 ?? string.Empty)
						.WithParameter("Phones", emailPhones.Item2 ?? string.Empty);
				} else if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b)
				{
					var innAndKpp = GetInnAndKpp(integrationInfo.Data);
					searchProcedure
						.WithParameter("Inn", innAndKpp.Item1 ?? string.Empty)
						.WithParameter("Kpp", innAndKpp.Item2 ?? string.Empty);
				}
			}
			
		}
		/// <summary>
		/// Возвращает ИНН и КПП из json-строки
		/// </summary>
		/// <param name="data">JObject</param>
		/// <returns>Кортеж с ИНН и КПП</returns>
		private Tuple<string, string> GetInnAndKpp(JObject data)
		{
			var inn = string.Empty;
			var kpp = string.Empty;
			try
			{
				var innToken = data.SelectToken(JName + ".taxRegistrationNumber");
				var kppToken = data.SelectToken(JName + ".companyRegistrationNumber");
				if(innToken != null)
				{
					inn = innToken.Value<string>();
				}
				if(kppToken != null)
				{
					kpp = kppToken.Value<string>();
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(inn, kpp);
		}
		/// <summary>
		/// Возвращает телефоны и мейлы из json-строки
		/// </summary>
		/// <param name="data">JObject</param>
		/// <returns>Кортеж с мейлами и телефонами</returns>
		private Tuple<string, string> GetEmailPhones(JObject data)
		{
			var emails = string.Empty;
			var phone = string.Empty;
			try
			{
				var communicationToken = data.SelectToken(JName + ".mainContactInfo.ContactInfo.records");
				if (communicationToken != null && communicationToken is JArray)
				{
					var communicationJArray = (JArray)communicationToken;
					emails = string.Join(",", communicationJArray
						.Where(x =>
						{
							var mailIdToken = x.SelectToken("ContactRecord.type.#ref.id");
							return mailIdToken != null && mailIdToken.Value<int>() == 1;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
					phone = string.Join(",", communicationJArray
						.Where(x =>
						{
							var mailIdToken = x.SelectToken("ContactRecord.type.#ref.id");
							return mailIdToken != null && mailIdToken.Value<int>() == 2;
						})
						.SelectMany(x => PhoneFormatHelper.ToAllFormats(x.SelectToken("ContactRecord.value").Value<string>())));
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phone);
		}
		/// <summary>
		/// Разширеный поиск существующего контрагента
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		/// <returns></returns>
		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			SetStoredProcedureNameByAccountInfo(integrationInfo);
			return base.IsEntityAlreadyExistAdvanced(integrationInfo);
		}
		/// <summary>
		/// В зависимости от бизнес-протокола контрагента устанавливает процедуру поиска
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		private void SetStoredProcedureNameByAccountInfo(IntegrationInfo integrationInfo)
		{
			var jObj = integrationInfo.Data;
			if(jObj != null)
			{
				var isB2b = jObj.SelectToken(JName + ".b2b").Value<bool>();
				var isB2c = jObj.SelectToken(JName + ".b2c").Value<bool>();
				if (isB2b)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
					return;
				} else if(isB2c)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
					return;
				} else
				{
					var isB2s = jObj.SelectToken(JName + ".b2s").Value<bool>();
					if(isB2s)
					{
						var innToken = jObj.SelectToken(JName + ".taxRegistrationNumber");
						var kppToken = jObj.SelectToken(JName + ".companyRegistrationNumber");
						if(innToken == null || kppToken == null || string.IsNullOrEmpty(innToken.Value<string>()) || string.IsNullOrEmpty(kppToken.Value<string>()))
						{
							var firstEmailToken = jObj.SelectToken(JName + ".mainContactInfo.ContactInfo.records[?(@.ContactRecord.type.#ref.id == 1)][last()]");
							_advancedSearchInfo.StoredProcedureName = firstEmailToken != null ?
								CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c :
								CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
						} else
						{
							_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
						}
					}
				}
			}
			_advancedSearchInfo.StoredProcedureName = string.Empty;
		}
	}

	[ImportHandlerAttribute("PersonProfile")]
	[ExportHandlerAttribute("Contact")]
	public class ContactHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;

		public ContactHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "PersonProfile";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.ContactConst.ContactSearchStoredProcedure
			};
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Action == CsConstant.IntegrationActionName.Update)
			{
				//http://tscore-task/browse/SKT-3478. Зачищаем чтобы синхронизация с деталью "Средства связи" не отработала
				integrationInfo.IntegratedEntity.ClearAllAddressField();
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			ContactEntityHelper.ResaveContactBirthday(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			ContactEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			try {
				Tuple<string, string> emailPhoneTuple = new Tuple<string, string>(string.Empty, string.Empty);
				if (integrationInfo != null && integrationInfo.Data != null)
				{
					emailPhoneTuple = GetEmailAndPhones(JName, integrationInfo.Data);
				}
				searchProcedure
					.WithParameter("Emails", emailPhoneTuple.Item1 ?? string.Empty)
					.WithParameter("Phones", emailPhoneTuple.Item2 ?? string.Empty);
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public static Tuple<string, string> GetEmailAndPhones(string jName, JObject jObj)
		{
			string emails = String.Empty;
			string phones = String.Empty;
			try
			{
				var communication = jObj.SelectToken(jName + ".contact.ContactInfo.records");
				if (communication != null && communication is JArray)
				{
					var jArrayCommunication = (JArray)communication;
					if (jArrayCommunication.Count() > 0)
					{
						emails = string.Join(",", jArrayCommunication.Where(x =>
						{
							var emailToken = x.SelectToken("ContactRecord.type.#ref.id");
							return emailToken != null && emailToken.Value<int>() == 1;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
						phones = string.Join(",", jArrayCommunication.Where(x =>
						{
							var emailToken = x.SelectToken("ContactRecord.type.#ref.id");
							return emailToken != null && emailToken.Value<int>() == 2;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
					}
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phones);
		}
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
		public override string HandlerName {
			get {
				return handlerName;
			}
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				var typeToken = integrationInfo.Data.SelectToken(JName + ".type.#ref.id");
				var activeToken = integrationInfo.Data.SelectToken(JName + ".active");
				if (typeToken != null && activeToken != null)
				{
					EntityName = GetEntityNameByTypeId(typeToken.Value<int>(), activeToken.Value<bool>());
					Mapper.UserConnection = integrationInfo.UserConnection;
				}
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public string GetEntityNameByTypeId(int typeId, bool active)
		{
			if (typeId != 4)
			{
				handlerName = active ? "TsAutoOwnerInfo" : "TsAutoOwnerHistory";
				return handlerName;
			}
			handlerName = active ? "TsAutoTechService" : "TsAutoTechHistory";
			return handlerName;
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				EntityName = integrationInfo.IntegratedEntity.GetType().Name;
				handlerName = EntityName;
			}
		}
		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			if (EntityName == "TsAutoTechService" || EntityName == "TsAutoOwnerInfo")
			{
				var desEntityName = EntityName == "TsAutoTechService" ? "TsAutoTechHistory" : "TsAutoOwnerHistory";
				var autoId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsAutomobileId");
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, desEntityName);
				esq.AddAllSchemaColumns();
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAutomobile", autoId));
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsExternalId", 0));
				var entities = esq.GetEntityCollection(integrationInfo.UserConnection);
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				foreach (var entity in entities)
				{
					integrator.IntegrateBpmEntity(entity);
				}
			}
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public void SyncAutomobileOwners(IntegrationInfo integrationInfo)
		{
			try
			{
				if (EntityName != "TsAutoOwnerInfo")
				{
					return;
				}
				var automobileId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsAutomobileId");
				if (automobileId != Guid.Empty)
				{
					var storedProcedure = new StoredProcedure(integrationInfo.UserConnection, "tsp_Integration_SynchronizeAutomobileOwners")
						.WithParameter(Column.Parameter(automobileId)) as StoredProcedure;
					storedProcedure.PackageName = integrationInfo.UserConnection.DBEngine.SystemPackageName;
					storedProcedure.Execute();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.SimpleLoggerErrorAction(e);
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			SyncAutomobileOwners(integrationInfo);
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
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public TsContactNotificationsHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsContactNotifications";
			JName = "NotificationProfile";
		}
		public override string SettingName {
			get {
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

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	[ImportHandlerAttribute("NotificationProfileAccount")]
	[ExportHandlerAttribute("NotificationProfileAccount")]
	public class TsAccountNotificationHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public TsAccountNotificationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAccountNotification";
			JName = "NotificationProfile";
		}
		public override string SettingName {
			get {
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

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	[ImportHandlerAttribute("Manager")]
	[ImportHandlerAttribute("ManagerGroup")]
	[ExportHandlerAttribute("SysAdminUnit")]
	public class SysAdminUnitHandler : EntityHandler
	{
		public ServiceUrlMaker UrlMaker;
		public override string HandlerName {
			get {
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
			} else
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
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			Mapper.UserConnection = integrationInfo.UserConnection;
			int externalId = 0;
			if (integrationInfo.IntegratedEntity != null)
			{
				externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsExternalId");
			}
			return Mapper.CheckIsExist("SysAdminUnit", integrationInfo.Data[JName].Value<int>("id"), "TsExternalId", externalId);
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
				var integrator = new ClientServiceIntegrator(userConnection);
				integrator.IntegrateBpmEntity(contactId, "Contact");
			}
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegratedEntity != null && JName == "Manager" && integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue") == (int)CsConstant.TSysAdminUnitType.User) {
				ResaveContact(integrationInfo.IntegratedEntity.PrimaryColumnValue, integrationInfo.UserConnection);
			}
		}
	}

	[ImportHandlerAttribute("CompanyProfileAssignment")]
	[ExportHandlerAttribute("TsAccountManagerGroup")]
	public class TsAccountManagerGroupHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
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

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			try
			{
				if (integrationInfo != null && integrationInfo.IntegratedEntity != null && integrationInfo.Data != null)
				{
					var automobile = integrationInfo.IntegratedEntity;
					var name = automobile.GetTypedColumnValue<string>("TsName");
					if (string.IsNullOrEmpty(name))
					{
						var mark = integrationInfo.Data.SelectToken("VehicleProfile.manufacturerName").Value<string>();
						var model = integrationInfo.Data.SelectToken("VehicleProfile.modelName").Value<string>();
						automobile.SetColumnValue("TsName", string.Concat(mark, " ", model));
						automobile.UpdateInDB(false);
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public void integratePassport(IntegrationInfo integrationInfo)
		{
			var automobileId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
			if (automobileId != Guid.Empty)
			{
				var helper = new ClientServiceIntegrator(integrationInfo.UserConnection);
				helper.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new VehiclePassportHandler(), false);
			}
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
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

		public override string ExternalIdPath {
			get {
				return "TsPassportExternalId";
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
					integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new TsAutomobileHandler(), false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "VehiclePassportHandler - ProcessResponse");
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			//Не создаем паспорт если еще нету автомоболя к которому можно его привязать
			return;
		}
	}

	[ImportHandlerAttribute("ContactInfo")]
	[ExportHandlerAttribute("ContactInfo")]
	public class ContactInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public ContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "ContactInfo";
		}

		public override string HandlerName {
			get {
				return JName;
			}
		}
	}

	[ImportHandlerAttribute("ContactAddress")]
	[ExportHandlerAttribute("ContactAddress")]
	public class AddressInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public AddressInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName {
			get {
				return "ContactAddress";
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
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
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
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				UpdateAddressFromDeliveryService(integrationInfo);
			}
		}
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				AddressHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
				ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.ParentEntity, integrationInfo.ParentEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}


	[ImportHandlerAttribute("AddressInfoAccount")]
	[ExportHandlerAttribute("AddressInfoAccount")]
	public class AddressInfoAccountHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public AddressInfoAccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName {
			get {
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

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				UpdateAddressFromDeliveryService(integrationInfo);
				AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.ParentEntity, integrationInfo.ParentEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			}
		}
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				AddressHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	[ImportHandlerAttribute("ContactCommunication")]
	[ExportHandlerAttribute("ContactCommunication")]
	public class ContactRecordHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public ContactRecordHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName {
			get {
				return "ContactCommunication";
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.ParentEntity, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
				try
				{
					integrationInfo.Data[JName]["useInContact"] = JToken.FromObject(true);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
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
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
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
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	[ImportHandlerAttribute("ContactRecordAccount")]
	[ExportHandlerAttribute("ContactRecordAccount")]
	public class AccountCommunicationHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public AccountCommunicationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName {
			get {
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
				try
				{
					integrationInfo.Data[JName]["useInAccount"] = JToken.FromObject(true);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.ParentEntity, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
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
				if (name != null && name.ToLower() == "shop")
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
				IntegrationLogger.Error(e, "importTransportationPointCompany " + integrationInfo.ToString());
				throw;
			}
		}
		private Guid GetTransportationIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if (transId == 0 || string.IsNullOrEmpty(transName))
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
				IntegrationLogger.Error(e, string.Format("{0} {1} {2}", "GetTransportationIdAndCreateIfNotExist", transId, transName));
			}
			return Guid.Empty;
		}

		private Guid GetTransportationCompanyIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if (transId == 0 || string.IsNullOrEmpty(transName))
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
				IntegrationLogger.Error(e, string.Format("{0} {1} {2}", "TsShipmentHandler.GetTransportationCompanyIdAndCreateIfNotExist", transId, transName));
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
				IntegrationLogger.Error(e, "OrderHandler.isCourierDeliveryMethod " + integrationInfo.ToString());
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
						//http://tscore-task/browse/SKT-3911
						//address = string.Format("{0}, {1}, {2}, {3}", street, building, appartament, address);
						ImportAddress(id, integrationInfo.UserConnection,
								GetGuidByValue("Country", country, userConnection, true),
								GetGuidByValue("Region", region, userConnection, true),
								GetGuidByValue("City", place, userConnection, true),
								GetGuidByValue("TsCounty", district, userConnection, true),
								IfNullThanEmpty(address),
								IfNullThanEmpty(zipCode),
								CsConstant.TsAddressType.Delivery,
								IfNullThanEmpty(street),
								IfNullThanEmpty(appartament),
								IfNullThanEmpty(building));
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[1 block] " + integrationInfo.ToString());
				}
				try
				{
					var OrderItemSum = GetOrderItemSum(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
					integrationInfo.IntegratedEntity.SetColumnValue("Amount", OrderItemSum);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[2 block] " + integrationInfo.ToString());
				}
				try
				{
					integrationInfo.IntegratedEntity.SetColumnValue("PaymentAmount", integrationInfo.IntegratedEntity.GetColumnValue("PrimaryPaymentAmount"));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[3 block] " + integrationInfo.ToString());
				}

				try
				{
					importTransportationPointCompany(integrationInfo);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[4 block] " + integrationInfo.ToString());
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
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[5 block] " + integrationInfo.ToString());
				}
				try
				{
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[6 block] " + integrationInfo.ToString());
				}
			}
		}

		public string IfNullThanEmpty(string text)
		{
			return string.IsNullOrEmpty(text) ? string.Empty : text;
		}
		public static Guid GetGuidByValue(string schemaName, string value, UserConnection userConnetion, bool createIfNotExist = false, string columnValue = "Name", string primaryColumn = "Id")
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
					if (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			if (createIfNotExist)
			{
				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnetion)
									.Into(schemaName)
									.Set(columnValue, Column.Parameter(value))
									.Set(primaryColumn, Column.Parameter(resultId)) as Insert;
				insert.Execute();
				return resultId;
			}
			return Guid.Empty;
		}

		public void ImportAddress(Guid orderId, UserConnection userConnection, Guid country, Guid region, Guid city, Guid tsCountry, string address, string zipCode, Guid addressType, string street, string apartment, string house)
		{
			try
			{
				var orderAddressId = GetOrderAddres(orderId, userConnection);
				if (orderAddressId == Guid.Empty)
				{
					InsertOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address, street, apartment, house);
				}
				else
				{
					UpdateOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address, street, apartment, house);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("orderId {0}, country {1}, region {2}, city {3}, tsCountry {4}, address {5}, zipCode {6}, addressType {7}", orderId, country, region, city, tsCountry, address, zipCode, addressType));
				throw;
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

		public Guid InsertOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address, string street, string apartment, string house)
		{
			var addressId = Guid.NewGuid();
			var columnsGuid = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var columnsString = new Dictionary<string, string>() {
				{ "Zip", zip },
				{ "TsStreet", street },
				{ "TsAppartment", apartment },
				{ "TsHouse", house },
				{ "Address", address }
			};
			var insert = new Insert(userConnection)
						.Into("TsOrderAddress")
						.Set("Id", Column.Parameter(addressId))
						.Set("Primary", Column.Parameter(true)) as Insert;
			columnsGuid
					.Where(x => x.Value != Guid.Empty)
					.ForEach(x => insert.Set(x.Key, Column.Parameter(x.Value)));
			columnsString
					.Select(x => x.Value == null ? new KeyValuePair<string, string>(x.Key, "") : x)
					.ForEach(x => insert.Set(x.Key, Column.Parameter(x.Value)));
			insert.Execute();
			return addressId;
		}
		public void UpdateOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address, string street, string apartment, string house)
		{
			var columnsGuid = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var columnsString = new Dictionary<string, string>() {
				{ "Zip", zip },
				{ "TsStreet", street },
				{ "TsAppartment", apartment },
				{ "TsHouse", house },
				{ "Address", address }
			};
			var update = new Update(userConnection, "TsOrderAddress")
						.Set("Primary", Column.Parameter(true))
						.Where("Id").In(new Select(userConnection)
										.Top(1)
										.Column("a", "Id")
										.From("TsOrderAddress").As("a")
										.Where("a", "TsOrderId").IsEqual(Column.Parameter(orderId))
										.OrderByDesc("a", "CreatedOn") as Select) as Update;
			columnsGuid
					.Where(x => x.Value != Guid.Empty)
					.ForEach(x => update.Set(x.Key, Column.Parameter(x.Value)));
			columnsGuid
					.Where(x => x.Value == Guid.Empty)
					.ForEach(x => update.Set(x.Key, Column.Const(null)));
			columnsString
					.Select(x => x.Value == null ? new KeyValuePair<string, string>(x.Key, "") : x)
					.ForEach(x => update.Set(x.Key, Column.Parameter(x.Value)));
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
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[1 block] " + integrationInfo.ToString());
				}

				try
				{
					var totalAmount = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TotalAmount");
					integrationInfo.IntegratedEntity.SetColumnValue("PrimaryTotalAmount", totalAmount);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[2 block] " + integrationInfo.ToString());
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
			try
			{
				var entity = integrationInfo.IntegratedEntity;
				var articul = integrationInfo.Data["OrderItem"]["oem"].Value<string>();
				var brand = integrationInfo.Data["OrderItem"]["brand"].Value<string>();
				integrationInfo.IntegratedEntity = GetProductByArticuleOrCreateNew(integrationInfo.UserConnection, articul, brand);
				integrationInfo.IntegratedEntity.SetDefColumnValues();
				Mapper.StartMappByConfig(integrationInfo, JName, IntegrationConfigurationManager.GetConfigItem(integrationInfo.UserConnection, "Product"));
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName);
				var productId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				entity.SetColumnValue("ProductId", productId);
				entity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "OrderProductHandler.CreateProduct " + integrationInfo.ToString());
			}
		}

		public Entity GetProductByArticuleOrCreateNew(UserConnection userConnection, string articule, string brand)
		{
			var productId = ProductEntityHelper.GetOrCreateProductByBrandAndOem(userConnection, brand, articule);
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Product");
			esq.AddAllSchemaColumns();
			return esq.GetEntity(userConnection, productId);
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
			try
			{
				var eom = integrationInfo.Data["ShipmentItem"]["oem"].Value<string>();
				var brand = integrationInfo.Data["ShipmentItem"]["brand"].Value<string>();
				var unitName = integrationInfo.Data["ShipmentItem"]["unitName"].Value<string>();
				var productId = ProductEntityHelper.GetOrCreateProductByBrandAndOem(integrationInfo.UserConnection, brand, eom,
								command =>
								{
									var unitId = JsonEntityHelper.GetColumnValues(integrationInfo.UserConnection, "Unit", "ShortName", unitName, "Id", 1).FirstOrDefault();
									if (unitId == null)
									{
										return;
									}
									if (command is Insert)
									{
										var insert = (Insert)command;
										insert.Set("UnitId", Column.Parameter(unitId));
									}
									else if (command is Update)
									{
										var update = (Update)command;
										update.Set("UnitId", Column.Parameter(unitId));
									}
								});
				integrationInfo.IntegratedEntity.SetColumnValue("TsProductId", productId);
				integrationInfo.IntegratedEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "UpdateProduct " + integrationInfo.ToString());
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
			var unitId = OrderHandler.GetGuidByValue("Unit", unitName, userConnection, false, "ShortName");
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
		public override string ExternalIdPath {
			get {
				return "TsContractBalanceId";
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public ContractBalanceHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contract";
			JName = "ContractBalance";
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			try
			{
				Mapper.UserConnection = integrationInfo.UserConnection;
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				integrationInfo.TsExternalVersionPath = ExternalVersionPath;
				JToken externalIdToken = integrationInfo.Data.SelectToken("ContractBalance.contract.#ref.id");
				if (externalIdToken == null)
				{
					return false;
				}
				int externalId = 0;
				if (integrationInfo.IntegratedEntity != null)
				{
					externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
				}
				return Mapper.CheckIsExist(EntityName, externalIdToken.Value<int>(), "TsExternalId", externalId);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return false;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			string externalIdPath = "TsExternalId";
			var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
			esq.AddAllSchemaColumns();
			esq.RowCount = 1;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data.SelectToken("ContractBalance.contract.#ref.id").Value<int>()));
			return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
		}
		public override void Create(IntegrationInfo integrationInfo)
		{
			try
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				integrationInfo.TsExternalVersionPath = ExternalVersionPath;
				var entitySchema = integrationInfo.UserConnection.EntitySchemaManager.GetInstanceByName(EntityName);
				integrationInfo.IntegratedEntity = entitySchema.CreateEntity(integrationInfo.UserConnection);
				integrationInfo.IntegratedEntity.SetDefColumnValues();
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection), IsJsonWithHeader);
				AfterMapping(integrationInfo);
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
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
				IntegrationLogger.Error(e, "ContractHandler.SetState " + integrationInfo.ToString());
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
				IntegrationLogger.Error(e, "ContractHandler.SetBussinesProtocol " + integrationInfo.ToString());
			}
		}

		public bool IsAccountLegal(Guid accountId, UserConnection userConnection)
		{
			var acountLegalSelect = new Select(userConnection)
							.Column("TsIsLawPerson").As("IsLegal")
							.From("Account")
							.Where("Id").IsEqual(Column.Parameter(accountId)) as Select;
			using (DBExecutor dbExecutor = acountLegalSelect.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = acountLegalSelect.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<bool>(reader, "IsLegal");
					}
				}
			}
			throw new Exception("IsAccountLegal throw Exception: No account with id = " + accountId.ToString());
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("StateId") == CsConstant.TsContractState.Vising && !IsAlreadyExported(integrationInfo);
		}
		/// <summary>
		/// Возвращает проинтегрирован ли договор
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		/// <returns>Признак проинтегрированости</returns>
		public bool IsAlreadyExported(IntegrationInfo integrationInfo)
		{
			if(integrationInfo.IntegratedEntity != null)
			{
				var externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
				return externalId > 0;
			}
			return false;
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
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierManagerInfo;
			}
		}

		public override string ExternalVersionPath {
			get {
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
		private AdvancedSearchInfo _advancedSearchInfo;

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["CounteragentContactInfo"]["positionFull"] = integrationInfo.Data["CounteragentContactInfo"]["position"];
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public bool isFindedAdvanced;
		public CounteragentContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "CounteragentContactInfo";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = "tsp_Integration_AdvancedSearch_Contact"
			};
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{

			var isClient = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsIsClient");
			if (!isClient)
			{
				var contactId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				var account = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
				return IsAccountHaveOrderServiceId(account, contactId, integrationInfo.UserConnection);
			}
			return true;
		}
		public bool IsAccountHaveOrderServiceId(Guid accountId, Guid contactId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
				.Column(Func.Count("Id")).As("count")
				.From("Account").As("a")
				.Where().OpenBlock("a", "PrimaryContactId").IsEqual(Column.Parameter(contactId)) as Select;
			if (accountId != Guid.Empty)
			{
				select.Or("a", "Id").IsEqual(Column.Parameter(accountId));
			}
			select
				.CloseBlock()
				.And("a", "TsOrderServiceId").IsNotEqual(Column.Const(0));
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
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				ContactEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Data != null)
			{
				if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".address").Value<string>()))
				{
					UpdateAddressFromDeliveryService(integrationInfo);
					UpdateLastAddressType(integrationInfo, IsAccountLegal(integrationInfo));
				}
			}
			ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			ContactEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			ReintegrateContactPersonProfile(integrationInfo);
			CreateContactCareer(integrationInfo);
		}

		private void CreateContactCareer(IntegrationInfo integrationInfo)
		{
			if (integrationInfo == null || integrationInfo.IntegratedEntity == null)
			{
				return;
			}
			try
			{
				var contactEntity = integrationInfo.IntegratedEntity;
				var accountId = contactEntity.GetTypedColumnValue<Guid>("AccountId");
				if (accountId != Guid.Empty)
				{
					var createCareerIfNeedSp = new StoredProcedure(integrationInfo.UserConnection,
								"tsp_IntegrationHandler_CreateContactCareer")
							.WithParameter("contactId", contactEntity.PrimaryColumnValue)
							.WithParameter("accountId", accountId)
							.WithOutputParameter("resultId", integrationInfo.UserConnection.DataValueTypeManager.GetInstanceByName("Guid"))
							.WithOutputParameter("changedIds", integrationInfo.UserConnection.DataValueTypeManager.GetInstanceByName("Text"))
						as StoredProcedure;
					createCareerIfNeedSp.PackageName = integrationInfo.UserConnection.DBEngine.SystemPackageName;
					createCareerIfNeedSp.Execute();
					var resultId = createCareerIfNeedSp.Parameters.GetByName("resultId").Value as Guid?;
					var changedIds = createCareerIfNeedSp.Parameters.GetByName("changedIds").Value as String;
					var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
					if (resultId.HasValue)
					{
						if(!CheckIsAccountIntegrateInClientService(integrationInfo.UserConnection, accountId))
						{
							integrator.IntegrateBpmEntity(accountId, "Account", new AccountHandler());
						}
						integrator.IntegrateBpmEntity(resultId.Value, "ContactCareer");
					}
					if (!string.IsNullOrEmpty(changedIds))
					{
						changedIds
							.Split(new char[] { ',' })
							.Select(x =>
							{
								Guid uId;
								Guid.TryParse(x, out uId);
								return uId;
							})
							.Where(x => x != Guid.Empty)
							.ForEach(careerId =>
							{
								integrator.IntegrateBpmEntity(careerId, "ContactCareer");
							});
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public bool CheckIsAccountIntegrateInClientService(UserConnection userConnection, Guid accountId)
		{
			try
			{
				if (accountId != Guid.Empty)
				{
					var select = new Select(userConnection)
							.Column(Func.Count("Id")).As("count")
							.From("Account")
							.Where("Id").IsEqual(Column.Parameter(accountId))
							.And("TsExternalId").IsGreater(Column.Const(0)) as Select;
					var count = select.ExecuteScalar<int>();
					return count > 0;
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return false;
		}
		private void ReintegrateContactPersonProfile(IntegrationInfo integrationInfo)
		{
			try
			{
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new ContactHandler());
				if(isFindedAdvanced)
				{
					//!!!KOSTYL
					IntegrationLocker.Unlock("Contact", integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsOrderServiceId"),
					"OrderService_CounteragentContactInfo");
					integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new CounteragentContactInfoHandler());
					//!!!KOSTYL
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public virtual bool? IsAccountLegal(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.IsColumnValueLoaded("AccountId"))
				{
					var accountId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
					if (accountId != Guid.Empty)
					{
						var select = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("TsIsLawPerson")
										.From("Account")
										.Where("Id").IsEqual(Column.Parameter(accountId)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									return reader.GetColumnValue<bool>("TsIsLawPerson");
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return null;
		}
		public virtual void UpdateLastAddressType(IntegrationInfo integrationInfo, bool? isAccountLegal)
		{
			if (!isAccountLegal.HasValue)
			{
				return;
			}
			try
			{
				var contactId = integrationInfo.IntegratedEntity.PrimaryColumnValue;
				if (contactId != Guid.Empty)
				{
					QueryColumnExpression addressTypeId = isAccountLegal.Value ? Column.Parameter(CsConstant.EntityConst.AddressType.Work) : Column.Parameter(CsConstant.EntityConst.AddressType.Delivery);
					var addressTypeUpdate = new Update(integrationInfo.UserConnection, "ContactAddress")
									.Set("AddressTypeId", addressTypeId)
									.Where("Id").IsEqual(new Select(integrationInfo.UserConnection)
															.Top(1)
															.Column("ca", "Id")
															.From("ContactAddress").As("ca")
															.Where("ca", "ContactId").IsEqual(Column.Parameter(contactId))
															.OrderByDesc("ca", "CreatedOn") as Select
									) as Update;
					addressTypeUpdate.Execute();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			try
			{
				Tuple<string, string> emailPhoneTuple = new Tuple<string, string>(string.Empty, string.Empty);
				if (integrationInfo != null && integrationInfo.Data != null)
				{
					emailPhoneTuple = GetEmailAndPhones(integrationInfo.Data);
				}
				searchProcedure
					.WithParameter("Emails", emailPhoneTuple.Item1 ?? string.Empty)
					.WithParameter("Phones", emailPhoneTuple.Item2 ?? string.Empty)
					.WithParameter("ExternalIdPath", ExternalIdPath);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public Tuple<string, string> GetEmailAndPhones(JObject jObj)
		{
			string emails = String.Empty;
			var phones = new List<string>();
			var emailToken = jObj.SelectToken(JName + ".email");
			if(emailToken != null)
			{
				emails = emailToken.Value<string>();
			}
			var phonesPath = new List<string>()
			{
				"mobilePhone",
				"primaryPhone",
				"workPhone"
			};
			phonesPath.ForEach(x =>
			{
				var phoneToken = jObj.SelectToken(JName + "." + x);
				if (phoneToken != null)
				{
					phones.AddRange(PhoneFormatHelper.ToAllFormats(phoneToken.Value<string>()));
				}
			});
			return new Tuple<string, string>(emails, string.Join(",", phones));
		}

		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			var result = base.IsEntityAlreadyExistAdvanced(integrationInfo);
			isFindedAdvanced = result;
			return result;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			isFindedAdvanced = false;
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}

	[ImportHandlerAttribute("Counteragent")]
	[ExportHandlerAttribute("Account")]
	public class CounteragentHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;

		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public bool isFindedAdvanced;

		public bool? isAccountHasOsId = null;
		public CounteragentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "Counteragent";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c
			};
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return
				integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath) > 0
				||
				!isAccountExported(integrationInfo) && isAccountContracted(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
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

		public override void Update(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.IsColumnValueLoaded(ExternalVersionPath))
			{
				var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalVersionPath);
				isAccountHasOsId = id > 0;
			};
			base.Update(integrationInfo);
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			SetBussinesProtocol(integrationInfo);
			UpdateAddressFromDeliveryService(integrationInfo);
			CreateContact(integrationInfo);
			AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			AccountEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			ReintegrateAccountCompanyProfile(integrationInfo);
			if(isFindedAdvanced)
			{
				ReintegrateAccountContacts(integrationInfo);
			}
		}

		private void ReintegrateAccountCompanyProfile(IntegrationInfo integrationInfo)
		{
			try
			{
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new AccountHandler());
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}

		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Data != null)
				{
					if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".address").Value<string>()))
					{
						AccountEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction, esq =>
						{
							esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "AddressType", CsConstant.EntityConst.AddressType.Legal));
						});
					}
					if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".locationAddress").Value<string>()))
					{
						AccountEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction, esq =>
						{
							esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "AddressType", CsConstant.EntityConst.AddressType.Fact));
						});
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
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
				IntegrationLogger.Error(e, "CounteragentHandler");
			}
		}

		public void CreateContact(IntegrationInfo integrationInfo)
		{
			if ((integrationInfo.Action == CsConstant.IntegrationActionName.Create || (isAccountHasOsId.HasValue && !isAccountHasOsId.Value)) && integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					var isB2c = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsB2C");
					if (isB2c)
					{
						AdvancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
						if(!ContactEntityHelper.FindContactByAccount(integrationInfo.IntegratedEntity, integrationInfo.UserConnection, (x) => AddParameterToSearchProcedure(integrationInfo, x), IntegrationLogger.SimpleLoggerErrorAction)) {
							ContactEntityHelper.CreateContactByAccount(integrationInfo.IntegratedEntity, integrationInfo.UserConnection, true, true, IntegrationLogger.SimpleLoggerErrorAction);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}

		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);

			try
			{
				if (!result.IsJTokenPathHasValue("Counteragent.taxRegistrationNumber"))
				{
					result.RemoveByPath("Counteragent.taxRegistrationNumberName");
					result.RemoveByPath("Counteragent.taxRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}

			try
			{
				if (!result.IsJTokenPathHasValue("Counteragent.companyRegistrationNumber"))
				{
					result.RemoveByPath("Counteragent.companyRegistrationNumberName");
					result.RemoveByPath("Counteragent.companyRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return result;
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			//Если контрагент только законтрактован то отправляем его в client service. Это происходит единожды
			ReintegrateContactsAndSelf(integrationInfo);
		}
		/// <summary>
		/// http://tscore-task/browse/SKT-4216
		/// Если у Counteragent не заполнено поле основной контакт, то инициируем переотправку всех контактов контрагента.
		/// Если у даного Контрагента будет заполнено поле "Основной контакт", то инициируем переотправку Контрагента в OrderService
		/// Гарантируется, что в рамках этой переотправки интеграция рекурсивно не начнет переотпраку, еще раз, по даному контрагенту
		/// </summary>
		/// <param name="integrationInfo">Информация о текущей транзакции интеграции</param>
		public void ReintegrateContactsAndSelf(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Action == CsConstant.IntegrationActionName.Create || isMainContactEmpty(integrationInfo))
			{
				var externalId = integrationInfo.IntegratedEntity.GetExternalIdValue(ExternalIdPath);
				//Добавляем новую блокировку которая не позволит переотпраку несколько раз
				LockerHelper.DoWithEntityLock(externalId, "Account", () =>
				{
					if (ReintegrateAccountContacts(integrationInfo))
					{
						try
						{
							//Снимаем блокировку перед переотправкой контрагента
							if (!IntegrationLocker.CheckUnLock("Account", externalId, "OrderService_Counteragent"))
							{
								IntegrationLocker.Unlock("Account", externalId, "OrderService_Counteragent");
							}
							var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
							integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new CounteragentHandler(), false);
						}
						catch (Exception e)
						{
							IntegrationLogger.Error(e);
						}
					}
				}, IntegrationLogger.SimpleLoggerErrorAction, "OrderService_Counteragent_ReintegrateSelf");
			}
		}
		/// <summary>
		/// Переотправляем контактов в ордерсервис
		/// </summary>
		/// <param name="integrationInfo"></param>
		/// <returns>Возвращает был ли отправлен </returns>
		public bool ReintegrateAccountContacts(IntegrationInfo integrationInfo)
		{
			try
			{
				var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				//Контакты из детали "Контакты" контрагента
				var accountContacts = ContactEntityHelper.GetAccountContacts(id, integrationInfo.UserConnection, IntegrationLogger.SimpleLoggerErrorAction);
				//Основной контакт
				var primaryContactId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("PrimaryContactId");
				if (primaryContactId != Guid.Empty)
				{
					accountContacts.Add(primaryContactId);
				}
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				foreach (var contactId in accountContacts)
				{
					integrator.IntegrateBpmEntity(contactId, "Contact", new CounteragentContactInfoHandler());
				}
				return primaryContactId != Guid.Empty;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return false;
		}
		private bool isMainContactEmpty(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data != null)
			{
				JToken mainContactToken = integrationInfo.Data.SelectToken(JName + ".mainContact.#ref.id");
				if (mainContactToken != null)
				{
					var mainContact = mainContactToken.Value<int>();
					if (mainContact > 0)
					{
						return false;
					}
				}
			}
			return true;
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			if (integrationInfo.Data != null)
			{
				if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c)
				{
					var emailPhones = GetEmailPhones(integrationInfo.Data);
					searchProcedure
						.WithParameter("Emails", emailPhones.Item1 ?? string.Empty)
						.WithParameter("Phones", emailPhones.Item2 ?? string.Empty)
						.WithParameter("ExternalIdPath", ExternalIdPath);
				} else if(AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b)
				{
					var innAndKpp = GetInnAndKpp(integrationInfo.Data);
					searchProcedure
						.WithParameter("Inn", innAndKpp.Item1 ?? string.Empty)
						.WithParameter("Kpp", innAndKpp.Item2 ?? string.Empty)
						.WithParameter("ExternalIdPath", ExternalIdPath);
				}
			}
			
		}

		private Tuple<string, string> GetEmailPhones(JObject data)
		{
			var emails = string.Empty;
			var phone = string.Empty;
			try
			{
				if (data != null)
				{
					var emailToken = data.SelectToken(JName + ".email");
					if (emailToken != null)
					{
						emails = emailToken.Value<string>();
					}
					var phoneToken = data.SelectToken(JName + ".phone");
					if (phoneToken != null)
					{
						phone = string.Join(",", PhoneFormatHelper.ToAllFormats(phoneToken.Value<string>()));
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phone);
		}
		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			SetStoredProcedureNameByAccountInfo(integrationInfo);
			var result = base.IsEntityAlreadyExistAdvanced(integrationInfo);
			isFindedAdvanced = result;
			return result;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			isFindedAdvanced = false;
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		private Tuple<string, string> GetInnAndKpp(JObject data)
		{
			var inn = string.Empty;
			var kpp = string.Empty;
			try
			{
				var innToken = data.SelectToken(JName + ".taxRegistrationNumber");
				var kppToken = data.SelectToken(JName + ".companyRegistrationNumber");
				if (innToken != null)
				{
					inn = innToken.Value<string>();
				}
				if (kppToken != null)
				{
					kpp = kppToken.Value<string>();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(inn, kpp);
		}
		private void SetStoredProcedureNameByAccountInfo(IntegrationInfo integrationInfo)
		{
			var jObj = integrationInfo.Data;
			if (jObj != null)
			{
				var isB2b = jObj.SelectToken(JName + ".legalEntity").Value<bool>();
				if (isB2b)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
					return;
				}
				else
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
					return;
				}
			}
			_advancedSearchInfo.StoredProcedureName = string.Empty;
		}
	}

	[ImportHandlerAttribute("AccountBillingInfo")]
	[ExportHandlerAttribute("AccountBillingInfo")]
	public class AccountBillingInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public override bool IsJsonWithHeader {
			get {
				return false;
			}
		}
		public AccountBillingInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountBillingInfo";
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


	[ImportHandlerAttribute("AccountAnniversary")]
	[ExportHandlerAttribute("AccountAnniversary")]
	public class AccountAnniversaryHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public override bool IsJsonWithHeader {
			get {
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
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public override bool IsJsonWithHeader {
			get {
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

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);

		}

		public override void AfterMapping(IntegrationInfo integrationInfo)
		{
			base.AfterMapping(integrationInfo);
			if (integrationInfo.ParentEntity != null)
			{
				var date = integrationInfo.IntegratedEntity.GetTypedColumnValue<DateTime>("Date");
				integrationInfo.ParentEntity.SetColumnValue("BirthDate", date);
				integrationInfo.ParentEntity.UpdateInDB(false);
			}
		}
	}
}