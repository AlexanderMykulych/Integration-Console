using System;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
	public class PrimaryExportParam
	{
		public string EntityName;
		public bool OnlyNew;
		public EntityHandler EntityHandler;
		public Action<Select> FilterAction;
		public string ExternalIdName;
		public bool WithRate;
		public int RateCount;

		public PrimaryExportParam(string entityName, bool onlyNew, EntityHandler entityHandler, string externalIdName, Action<Select> filterAction = null, int rateCount = 100)
		{
			EntityName = entityName;
			OnlyNew = onlyNew;
			EntityHandler = entityHandler;
			FilterAction = filterAction;
			ExternalIdName = externalIdName;
			RateCount = rateCount;
		}
	}
}