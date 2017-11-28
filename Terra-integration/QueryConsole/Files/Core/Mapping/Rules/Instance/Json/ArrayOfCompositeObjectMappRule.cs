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
	[RuleAttribute("ArrayOfCompositeObject")]
	public class ArrayOfCompositeObjectMappRule : IMappRule
	{
		public ArrayOfCompositeObjectMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			if (info.integrationType == CsConstant.TIntegrationType.ExportResponseProcess && !info.config.SaveOnResponse)
			{
				return;
			}
			if (info.json is JArray)
			{
				var jArray = (JArray)info.json;
				var handlerName = info.config.HandlerName;
				var integrator = new IntegrationEntityHelper();
				var integrateIds = new List<QueryColumnExpression>();
				try
				{
					foreach (JToken jArrayItem in jArray)
					{
						JObject jObj = jArrayItem as JObject;
						handlerName = handlerName ?? jObj.Properties().First().Name;
						CsConstant.IntegrationInfo objIntegrInfo = null;
						objIntegrInfo.ParentEntity = info.entity;
						integrator.IntegrateEntity(objIntegrInfo);
						if (info.config.DeleteBeforeExport)
						{
							integrateIds.Add(Column.Parameter(objIntegrInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id")));
						}
					}
					if (info.config.DeleteBeforeExport)
					{
						EntitySchema entitySchema = info.userConnection.EntitySchemaManager.GetInstanceByName(info.config.TsDestinationName);
						string destColumnName = JsonEntityHelper.GetSqlNameByEntity(entitySchema, info.config.TsDestinationPath);
						if (integrateIds != null && integrateIds.Any())
						{
							var idSelect = new Select(info.userConnection)
											.Column("Id")
											.From(info.config.TsDestinationName)
											.Where("Id").Not().In(integrateIds)
											.And(destColumnName).In(
												new Select(info.userConnection)
													.Column(destColumnName)
													.From(info.config.TsDestinationName)
													.Where("Id").In(integrateIds)
											)
											as Select;
							if (!string.IsNullOrEmpty(info.config.BeforeDeleteMacros))
							{
								MacrosFactory.ExecuteBeforeDeleteMacros(info.config.BeforeDeleteMacros, idSelect, info.userConnection);
							}
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where("Id").In(idSelect) as Delete;
							delete.Execute();
						}
						else
						{
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where(destColumnName).IsEqual(Column.Parameter(info.entity.GetColumnValue(info.config.TsSourcePath))) as Delete;
							delete.Execute();
						}
					}
				}
				catch (Exception e)
				{
					throw new Exception("Mapp Rule arrayofcompositobject, import", e);
				}
			}
		}
		public void Export(RuleExportInfo info)
		{
			try
			{
				if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName))
				{
					//var srcEntity = info.entity;
					//var dscValue = srcEntity.GetColumnValue(info.config.TsSourcePath);
					//string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
					//var resultJObjs = JsonEntityHelper.GetCompositeJObjects(dscValue, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection);
					//if (resultJObjs.Any()) {
					//	var jArray = (info.json = new JArray()) as JArray;
					//	resultJObjs.ForEach(x => jArray.Add(x));
					//} else {
					//	info.json = null;
					//}
				}
			}
			catch (Exception e)
			{
				throw new Exception("Mapp Rule arrayofcompositobject, export", e);
			}
		}
	}
}