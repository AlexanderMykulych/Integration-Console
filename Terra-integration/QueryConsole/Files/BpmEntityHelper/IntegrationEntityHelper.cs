﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;
using Terrasoft.Core.DB;
using System.Data;
using IntegrationInfo = Terrasoft.TsConfiguration.CsConstant.IntegrationInfo;

namespace Terrasoft.TsConfiguration
{
	public class IntegrationEntityHelper
	{
		private static List<Type> IntegrationEntityTypes { get; set; }
		private static Dictionary<Type, EntityHandler> EntityHandlers { get; set; }
		 

				public IntegrationEntityHelper()
		{
			EntityHandlers = new Dictionary<Type, EntityHandler>();
		}
		 

				/// <summary>
		/// Експортирует или импортирует объекты в зависимости от настроек
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		public void IntegrateEntity(IntegrationInfo integrationInfo)
		{
			ExecuteHandlerMethod(integrationInfo, GetIntegrationHandler(integrationInfo));
		}

		/// <summary>
		/// В зависимости от типа интеграции возвращает соответсвенный атрибут
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public Type GetAttributeType(IntegrationInfo integrationInfo)
		{
			return GetAttributeType(integrationInfo.IntegrationType);
		}
		public Type GetAttributeType(CsConstant.TIntegrationType integrationType)
		{
			switch (integrationType)
			{
				case CsConstant.TIntegrationType.Import:
					return typeof(ImportHandlerAttribute);
				case CsConstant.TIntegrationType.Export:
				case CsConstant.TIntegrationType.ExportResponseProcess:
					return typeof(ExportHandlerAttribute);
				default:
					return typeof(ExportHandlerAttribute);
			}
		}

		/// <summary>
		/// Возвращает все классы помеченые атрибутами интеграции которые розмещены в пространстве имен Terrasoft.Configuration
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public List<Type> GetIntegrationTypes(IntegrationInfo integrationInfo)
		{
			return GetIntegrationTypes(integrationInfo.IntegrationType);
		}

		public List<Type> GetIntegrationTypes(CsConstant.TIntegrationType integrationType)
		{
			if (IntegrationEntityTypes != null && IntegrationEntityTypes.Any())
			{
				return IntegrationEntityTypes;
			}
			var attributeType = GetAttributeType(integrationType);
			var assembly = typeof(IntegrationServiceIntegrator).Assembly;
			return IntegrationEntityTypes = assembly.GetTypes().Where(x =>
			{
				var attributes = x.GetCustomAttributes(attributeType, true);
				return attributes != null && attributes.Length > 0;
			}).ToList();
		}

		/// <summary>
		/// Возвращает объект который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public IIntegrationEntityHandler GetIntegrationHandler(IntegrationInfo integrationInfo) {
			var attributeType = GetAttributeType(integrationInfo);
			var types = GetIntegrationTypes(integrationInfo);
			if (integrationInfo.Handler != null) {
				return integrationInfo.Handler;
			}
			var handlerName = integrationInfo.EntityName;
			foreach (var type in types) {
				var attributes = type.GetCustomAttributes(attributeType, true);

				foreach (IntegrationHandlerAttribute attribute in attributes) {

					if (attribute != null && attribute.EntityName == handlerName) {
						if (EntityHandlers.ContainsKey(type)) {
							return EntityHandlers[type];
						}
						var entityHandler = Activator.CreateInstance(type) as EntityHandler;
						if (EntityHandlers.ContainsKey(type))
						{
							return EntityHandlers[type];
						}
						EntityHandlers.Add(type, entityHandler);
						return entityHandler;
					}
				}
			}
			return null;
		}

		public List<EntityHandler> GetAllIntegrationHandler(string entityName, CsConstant.TIntegrationType integrationType) {
			var result = new List<EntityHandler>();

			var attributeType = GetAttributeType(integrationType);
			var types = GetIntegrationTypes(integrationType);
			foreach (var type in types) {
				var attributes = type.GetCustomAttributes(attributeType, true);

				foreach (IntegrationHandlerAttribute attribute in attributes) {

					if (attribute != null && attribute.EntityName == entityName) {
						if (EntityHandlers.ContainsKey(type))
						{
							result.Add((EntityHandler)EntityHandlers[type]);
						}
						else
						{
							var entityHandler = Activator.CreateInstance(type) as EntityHandler;
							EntityHandlers.Add(type, entityHandler);
							result.Add(entityHandler);
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// В зависимости от настройки интеграции, выполняет соответсвенный метод объкта, который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <param name="handler">объект, который отвечает за интеграцию конкретной сущности</param>
		public void ExecuteHandlerMethod(IntegrationInfo integrationInfo, IIntegrationEntityHandler handler)
		{
			if(integrationInfo.Handler == null && handler is EntityHandler) {
				integrationInfo.Handler = (EntityHandler)handler;
			}
			if (handler != null)
			{
				try
				{
					if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
					{
						if(handler.IsExport(integrationInfo)) {
							var result = new CsConstant.IntegrationResult(CsConstant.IntegrationResult.TResultType.Success, handler.ToJson(integrationInfo));
							integrationInfo.Result = result;
						}
						return;
					} else if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess)
					{
						handler.ProcessResponse(integrationInfo);
						return;
					}
					if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
					{
						if (!handler.IsEntityAlreadyExist(integrationInfo))
						{
							handler.Create(integrationInfo);
						}
						else
						{
							var result = new CsConstant.IntegrationResult(CsConstant.IntegrationResult.TResultException.OnCreateEntityExist);
							integrationInfo.Result = result;
							return;
						}
					}
					else if (integrationInfo.Action == CsConstant.IntegrationActionName.Update)
					{
						if (handler.IsEntityAlreadyExist(integrationInfo))
						{
							handler.Update(integrationInfo);
						}
						else
						{
							handler.Create(integrationInfo);
						}
					}
					else if (integrationInfo.Action == CsConstant.IntegrationActionName.Delete)
					{
						handler.Delete(integrationInfo);
					}
					else
					{
						handler.Unknown(integrationInfo);
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "ExecuteHandlerMethod");
				}
			}
		}
		 

		public static bool isEntityAlreadyIntegrated(Entity entity)
		{
			return entity.IsColumnValueLoaded("TsExternalId") && entity.GetTypedColumnValue<int>("TsExternalId") != 0;
		}
	}
}
