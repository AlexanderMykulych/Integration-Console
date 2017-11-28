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
	[RuleAttribute("ManyToMany")]
	public class ManyToManyMappRule : IMappRule
	{
		public ManyToManyMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			//if (info.json != null && info.json.HasValues)
			//{
			//	var jArray = info.json as JArray;
			//	foreach (var refItem in jArray)
			//	{
			//		var item = refItem[JsonEntityHelper.RefName];
			//		var externalId = int.Parse(item["id"].ToString());
			//		var type = item["type"];
			//		Tuple<Dictionary<string, string>, Entity> tuple = JsonEntityHelper.GetEntityByExternalId(info.config.TsExternalSource, externalId, info.userConnection, false, info.config.TsExternalPath);
			//		Dictionary<string, string> columnDict = tuple.Item1;
			//		Entity entity = tuple.Item2;
			//		if(entity != null) {
			//			if(!JsonEntityHelper.isEntityExist(info.config.TsDestinationName, info.userConnection, new Dictionary<string,object>() {
			//				{ info.config.TsDestinationPathToSource, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath) },
			//				{ info.config.TsDestinationPathToExternal, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]) }
			//			})) {
			//				var schema = info.userConnection.EntitySchemaManager.GetInstanceByName(info.config.TsDestinationName);
			//				var destEntity = schema.CreateEntity(info.userConnection);
			//				var firstColumn = schema.Columns.GetByName(info.config.TsDestinationPathToExternal).ColumnValueName;
			//				var secondColumn = schema.Columns.GetByName(info.config.TsDestinationPathToSource).ColumnValueName;
			//				destEntity.SetColumnValue(firstColumn, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]));
			//				destEntity.SetColumnValue(secondColumn, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath));
			//				destEntity.Save(false);
			//			}
			//		}
			//	}
			//}
		}
		public void Export(RuleExportInfo info)
		{

		}
	}
}