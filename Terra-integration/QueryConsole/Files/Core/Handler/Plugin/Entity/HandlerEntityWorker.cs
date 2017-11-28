using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public class HandlerEntityWorker : IHandlerEntityWorker
	{
		//Log key=Handler Util
		public Entity CreateEntity(UserConnection userConnection, string entityName)
		{
			var entitySchema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			var entity = entitySchema.CreateEntity(userConnection);
			entity.SetDefColumnValues();
			return entity;
		}
		//Log key=Handler Util
		public void SaveEntity(Entity entity, string jName, Action OnSuccess, Action OnError)
		{
			try
			{
				LoggerHelper.DoInLogBlock("Save Entity", () =>
				{
					IntegrationLogger.Info(string.Format("Entity\nType: {0}\nPrimary Value: \"{1}\"\nDisplay Value: \"{2}\"", entity.GetType(), entity.PrimaryColumnValue, entity.PrimaryDisplayColumnValue));
					try
					{
						entity.Save(false);
						OnSuccess();
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
						OnError();
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		//Log key=Handler Util
		public Entity GetEntityByExternalId(UserConnection userConnection, string entityName, string externalIdPath, string externalId)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			esq.AddAllSchemaColumns();
			esq.RowCount = 1;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, externalId));
			return esq.GetEntityCollection(userConnection).FirstOrDefault();
		}
		//Log key=Handler Util
		public Entity GetEntityById(UserConnection userConnection, string entityName, Guid id)
		{
			var entityEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			entityEsq.AddAllSchemaColumns();
			return entityEsq.GetEntity(userConnection, id);
		}
	}
}