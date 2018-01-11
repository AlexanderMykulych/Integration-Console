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
	[RuleAttribute("CompositObject")]
	public class CompositMappRule : IMappRule
	{
		public CompositMappRule()
		{
			
		}
		public void Import(RuleImportInfo info)
		{
			//var integrator = new BaseServiceIntegrator(info.userConnection);
			//integrator.IntegrateServiceEntity(info.json);
		}
		public void Export(RuleExportInfo info)
		{
			var userConnection = ObjectFactory
				.Get<IConnectionProvider>()
				.Get<UserConnection>();
			
			var entityId = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
			if (entityId != Guid.Empty)
			{
				var settingProvider = ObjectFactory.Get<ISettingProvider>();
				var config = settingProvider.SelectFirstByType<ConfigSetting>(x => x.Id == info.config.HandlerConfigId);
				var entityHelper = new IntegrationEntityHelper();
				var handler = entityHelper.GetAllIntegrationHandler(new List<ConfigSetting>() { config }).FirstOrDefault();
				if (handler != null)
				{
					var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, config.EntityName);
					esq.AddAllSchemaColumns();
					var entity = esq.GetEntity(userConnection, entityId);
					if (entity != null)
					{
						var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(entity);
						var resultJson = handler.ToJson(integrationInfo);
						info.json.SetObject(resultJson.GetObject());
						return;
					}
				}
			}
			info.json.SetObject(null);
		}
	}
}