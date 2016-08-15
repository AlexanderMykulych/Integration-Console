using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsConfiguration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
	public class ArrayOfCompositeObjectMappRule: BaseMappRule
	{
		public ArrayOfCompositeObjectMappRule()
		{
			_type = "arrayofcompositobject";
		}
		public override void Import(RuleImportInfo info)
		{
			if (info.integrationType == CsConstant.TIntegrationType.ExportResponseProcess)
			{
				//var dscValue = srcEntity.GetColumnValue(info.config.TsSourcePath);
				//var resultJObjs = JsonEntityHelper.GetCompositeJObjects(dscValue, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection);
				//if (info.json is JArray)
				//{
				//	var jArray = (JArray)info.json;
				//	foreach (JToken jArrayItem in jArray)
				//	{
				//		JObject jObj = jArrayItem as JObject;
				//		var integrator = new IntegrationEntityHelper();
				//		var objIntegrInfo = new IntegrationInfo(jObj, info.userConnection, info.integrationType, null, jObj.Properties().First().Name, info.action);
				//		integrator.IntegrateEntity(objIntegrInfo);
				//	}
				//}
			}
			else
			{
				if (info.json is JArray)
				{
					var jArray = (JArray)info.json;
					var handlerName = info.config.HandlerName;
					var integrator = new IntegrationEntityHelper();
					List<QueryColumnExpression> deleteIds = new List<QueryColumnExpression>();
					try
					{
						foreach (JToken jArrayItem in jArray)
						{
							JObject jObj = jArrayItem as JObject;
							handlerName = handlerName ?? jObj.Properties().First().Name;
							var objIntegrInfo = new CsConstant.IntegrationInfo(jObj, info.userConnection, info.integrationType, null, handlerName, info.action);
							objIntegrInfo.ParentEntity = info.entity;
							integrator.IntegrateEntity(objIntegrInfo);
							if(info.config.DeleteBeforeExport) {
								deleteIds.Add(Column.Parameter(objIntegrInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id")));
							}
						}
						if(info.config.DeleteBeforeExport && deleteIds != null && deleteIds.Any()) {
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where("Id").Not().In(deleteIds) as Delete;
							delete.Execute();
						}
					} catch(Exception e) {
						//TODO:
					}

				}
			}

		}
		public override void Export(RuleExportInfo info)
		{
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName))
			{
				var srcEntity = info.entity;
				var dscValue = srcEntity.GetColumnValue(info.config.TsSourcePath);
				string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
				var resultJObjs = JsonEntityHelper.GetCompositeJObjects(dscValue, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection);
				if(resultJObjs.Any()) {
					var jArray = (info.json = new JArray()) as JArray;
					resultJObjs.ForEach(x => jArray.Add(x));
				} else {
					info.json = null;
				}
			}
		}
	}
}
