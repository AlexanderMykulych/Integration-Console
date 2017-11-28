using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core.Process;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace Terrasoft.TsIntegration.Configuration
{
	public class TriggerCheckerItem : ITriggerCheckerItem<Entity>
	{
		private TriggerSetting _setting;
		private EntitySchemaQueryFilterCollection _filter;

		public TriggerCheckerItem(TriggerSetting setting)
		{
			_setting = setting;
		}

		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			if (CheckEventName(eventName) && eventInfo.SchemaName == _setting.EntitySchema)
			{
				var userConnection = eventInfo.UserConnection;
				var esq = new EntitySchemaQuery(eventInfo.Schema);
				esq.UseAdminRights = false;
				var filters = GetEsqFilterCollection(esq, userConnection);
				if (filters != null && filters.Count > 0)
				{
					esq.Filters.Add(filters);
					esq.AddColumn(eventInfo.Schema.PrimaryColumn.Name);
					var resultEntity = esq.GetEntity(userConnection, eventInfo.PrimaryColumnValue);
					if (resultEntity != null)
					{
						onMath(_setting);
						return true;
					}
				}
				else
				{
					onMath(_setting);
				}
			}
			return false;
		}

		public TriggerSetting GetSetting()
		{
			return _setting;
		}

		public EntitySchemaQueryFilterCollection GetEsqFilterCollection(EntitySchemaQuery esq, UserConnection userConnection)
		{
			if (_filter == null)
			{
				var processDataContractFilterConverter = ClassFactory.Get<ProcessDataContractFilterConverter>(new ConstructorArgument("userConnection", userConnection));
				_filter = processDataContractFilterConverter.ConvertToEntitySchemaQueryFilterItem(esq, _setting.Filter) as EntitySchemaQueryFilterCollection;
			}
			return _filter;
		}
		protected virtual bool CheckEventName(string eventName)
		{
			return (_setting.OnUpdate && eventName == "update") || (_setting.OnInsert && eventName == "insert");
		}
	}
}
