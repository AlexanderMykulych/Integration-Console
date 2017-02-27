using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public class PrimaryImportScenario
	{
		public List<string> ServicesObject = new List<string>();
		public string StartObjName;
		public int Skip;
		public bool WithUpdateExist;
		public int BatchLimit;
		public UserConnection UserConnection;
		public Dictionary<string, string> Filters = new Dictionary<string, string>();
		public PrimaryImportScenario(UserConnection userConnection, string startObjName, int skip, bool withUpdateExist, int batchLimit, params string[] servicesObjectNames)
		{
			StartObjName = startObjName;
			Skip = skip;
			ServicesObject.AddRange(servicesObjectNames);
			UserConnection = userConnection;
			WithUpdateExist = withUpdateExist;
			BatchLimit = batchLimit;
		}
		public void Run()
		{
			if(!ServicesObject.Any())
			{
				return;
			}
			int skip = Skip;
			int startServiceObjIndex = 0;
			if(!string.IsNullOrEmpty(StartObjName))
			{
				startServiceObjIndex = ServicesObject.IndexOf(StartObjName);
			}
			for(int i = 0; i < ServicesObject.Count; i++)
			{
				if (i >= startServiceObjIndex)
				{
					var name = ServicesObject[i];
					string filter = string.Empty;
					if(Filters.ContainsKey(name))
					{
						filter = Filters[name];
					}
					ImportServiceObject(name, skip, filter);
					if (skip > 0)
					{
						skip = 0;
					}
				}
			}
		}

		internal PrimaryImportScenario AddFilters(string entityName, string filter)
		{
			if(!Filters.ContainsKey(entityName))
			{
				Filters.Add(entityName, filter);
				return this;
			}
			Filters[entityName] = filter;
			return this;
		}

		public void ImportServiceObject(string serviceObjName, int skip, string filter = null)
		{
			var options = new PrimaryImportParam(serviceObjName, UserConnection, WithUpdateExist, 0, BatchLimit, false, skip, filter: filter);
			var importProvider = new PrimaryImportProvider(options);
			importProvider.Run();
		}
	}
}
