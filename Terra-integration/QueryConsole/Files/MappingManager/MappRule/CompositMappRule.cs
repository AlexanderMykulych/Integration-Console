﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsConfiguration;

namespace Terrasoft.TsConfiguration
{
	public class CompositMappRule: BaseMappRule
	{
		public CompositMappRule()
		{
			_type = "compositobject";
		}
		public override void Import(RuleImportInfo info)
		{
			var integrator = new IntegrationEntityHelper();
			//var objIntegrInfo = new IntegrationInfo(jToken, integrationInfo.UserConnection, integrationInfo.IntegrationType, null, null, integrationInfo.Action);
			var jObject = info.json as JObject;
			var objIntegrInfo = new CsConstant.IntegrationInfo(jObject, info.userConnection, info.integrationType, null, jObject.Properties().First().Name, info.action);
			integrator.IntegrateEntity(objIntegrInfo);
		}
		public override void Export(RuleExportInfo info)
		{
			object resultJObj = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName, info.config.JSourceName))
			{
				var srcEntity = info.entity;
				var dscId = srcEntity.GetColumnValue(info.config.TsSourcePath);
				string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
				resultJObj = JsonEntityHelper.GetCompositeJObjects(dscId, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection, 1).FirstOrDefault();
			}
			info.json = resultJObj as JToken;
		}
	}
}
