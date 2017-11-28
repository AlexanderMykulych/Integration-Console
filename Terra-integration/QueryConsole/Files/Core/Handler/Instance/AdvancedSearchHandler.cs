using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsIntegration.Configuration
{
	[IntegrationHandlerAttribute("AdvancedSearchHandler")]
	public class AdvancedSearchHandler: DefaultEntityHandler
	{
		public AdvancedSearchHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		public override bool IsEntityAlreadyExist(CsConstant.IntegrationInfo integrationInfo)
		{
			bool result = false;
			LoggerHelper.DoInLogBlock("Handler: Advanced Search", () =>
			{
				var searchEntityName = GetHandlerConfigValue("SearchEntity");
				var searchEntityColumn = GetHandlerConfigValue("SearchEntityColumn");
				var valuePath = GetHandlerConfigValue("SearchPathValue");
				var value = integrationInfo.Data.GetProperty<string>(valuePath);
				if (!string.IsNullOrEmpty(value))
				{
					result = isEntityExist(searchEntityName, searchEntityColumn, value, integrationInfo.UserConnection);
				}
			});
			if (!result)
			{
				throw new EntityNotFoundException();
			}
			return bool.Parse(GetHandlerConfigValue("DefaultResult"));
		}
		//Log Key = Handler
		protected virtual bool isEntityExist(string entityName, string columnName, string value, UserConnection userConnection)
		{
			var select = new Select(userConnection)
				.Column(Func.Count(Column.Asterisk()))
				.From(entityName)
				.Where(columnName).IsEqual(Column.Parameter(value)) as Select;
			return select.ExecuteScalar<int>() > 0;
		}
	}
}
