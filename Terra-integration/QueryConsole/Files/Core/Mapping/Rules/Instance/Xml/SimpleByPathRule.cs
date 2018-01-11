using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration
{
	[RuleAttribute("SimpleByPath", TIntegrationObjectType.Xml)]
	public class SimpleByPathRule : IMappRule
	{
		//Log Name=Simple By Path Mapp, Key=MappingRule
		public void Export(RuleExportInfo info)
		{
			var userConnection = ObjectFactory
				.Get<IConnectionProvider>()
				.Get<UserConnection>();
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, info.entity.SchemaName);
			esq.RowCount = 1;
			var column = esq.AddColumn(info.config.TsSourcePath);
			Entity entity = null;
			bool loadById = true;
			if (!string.IsNullOrEmpty(info.config.OrderColumn))
			{
				var orderColumn = esq.AddColumn(info.config.OrderColumn);
				orderColumn.OrderPosition = 0;
				orderColumn.OrderDirection = info.config.OrderType;
				loadById = false;
			}
			if (!string.IsNullOrEmpty(info.config.TsTag))
			{
				Dictionary<string, string> filterColumns = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
				foreach (var filterColumn in filterColumns)
				{
					object filterValue = filterColumn.Value;
					string key = filterColumn.Key;
					if (filterColumn.Value.StartsWith("$"))
					{
						filterValue = info.entity.GetColumnValue(filterColumn.Value.Substring(1));
					}
					esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filterColumn.Key, filterValue));
				}
				loadById = false;
			}
			if (loadById)
			{
				entity = esq.GetEntity(userConnection, info.entity.PrimaryColumnValue);
			}
			else
			{
				entity = esq.GetEntityCollection(userConnection).FirstOrDefault();
			}
			if (entity != null)
			{
				var nameColumn = entity.Schema.Columns.GetByName(column.Name);
				var value = entity.GetColumnValue(nameColumn.Name);
				var simpleResult = value != null ? JsonEntityHelper.GetSimpleTypeValue(value) : null;
				if (!string.IsNullOrEmpty(info.config.MacrosName))
				{
					simpleResult = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, simpleResult);
					if (simpleResult is DateTime)
					{
						simpleResult = ((DateTime)simpleResult).ToString("dd-MM-yyyy");
					}
				}

				info.json.FromObject(simpleResult);
			}
		}

		public void Import(RuleImportInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
