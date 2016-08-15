using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
	public abstract class BaseIntegratorTester
	{
		public BaseServiceIntegrator Integrator;
		public List<string> BpmEntitiesName;
		public List<string> ServiceEntitiesName;
		public UserConnection UserConnection;
		public int Limit;
		public int Skip;
		public Action AfterIntegrate;

		public BaseIntegratorTester(UserConnection userConnection) {
			BpmEntitiesName = InitEntitiesName();
			ServiceEntitiesName = InitServiceEntitiesName();
			UserConnection = userConnection;
			Integrator = CreateIntegrator();
		}
		public void ImportAllBpmEntity() {
			foreach(var entityName in BpmEntitiesName) {
				ClonsoleGreen(entityName + " Start:");
				try {
					var collection = GetEntitiesBySchemaNames(entityName, false);
					foreach(var entity in collection) {
						ImportBpmEntity(entity);
					}
				} catch(Exception e) {
					ClonsoleGreen(" Error: " + e.Message);
				}
				ClonsoleGreen(entityName + " End:");
			}
		}
		public EntityCollection GetEntitiesBySchemaNames(string name, bool withoutIntegrated = true) {
			var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, name);
			esq.RowCount = 1;
			esq.AddAllSchemaColumns();
			var dateColumn = esq.AddColumn("CreatedOn");
			dateColumn.OrderByDesc();
			//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", "Наша компания"));
			///*esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("a37*/d31d9-4b81-4c40-925c-1b1d658e926d")));
			//if(name == "Contact") {
			//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("3aa33fe9-1685-4ad5-8a3e-d5f8aabc280d")));
			//}
			if (name == "ContactCareer") {
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("6D20D35D-82B9-4CFD-9D18-670003D79AB7")));
			}
			if (withoutIntegrated) {
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, CsConstant.ServiceColumnInBpm.Identifier, 0));
			}
			if(name == "SysAdminUnit") {
				//Manager Group
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "SysAdminUnitTypeValue", 4));
				////Lrs Moskov
				////esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("735379a1-301a-411a-aa98-2e65651961ac")));
				////Sp
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("220f42be-85d4-48b6-8fa1-1fa3c992fc64")));

				//Manager
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "SysAdminUnitTypeValue", 4));
			}
			if(name=="Contact") {
				//
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("E7954750-4215-4228-B26F-940513A4D082")));
			}
			return esq.GetEntityCollection(UserConnection);
		}
		public void ExportServiceEntity(string name, Action afterIntegrate = null)
		{
			AfterIntegrate = afterIntegrate;
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = Limit.ToString();
			serviceRequestInfo.Skip = Skip.ToString();
			serviceRequestInfo.AfterIntegrate = AfterIntegrate;
			Integrator.GetRequest(serviceRequestInfo);
		}
		public void ExportServiceEntity(string name, int id, Action afterIntegrate = null)
		{
			AfterIntegrate = afterIntegrate;
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = Limit.ToString();
			serviceRequestInfo.Skip = Skip.ToString();
			serviceRequestInfo.ServiceObjectId = id.ToString();
			serviceRequestInfo.AfterIntegrate = AfterIntegrate;
			Integrator.GetRequest(serviceRequestInfo);
		}
		public void ExportAllServiceEntities(int limit = 10, int skip = 0) {
			Limit = limit;
			Skip = skip;
			foreach(var name in ServiceEntitiesName) {
				ClonsoleGreen("Start: " + name);
				try {
					ExportServiceEntity(name);
				} catch(Exception e) {
					ClonsoleGreen("Error: " + e.Message);
				}
				ClonsoleGreen("End: " + name);
			}
		}
		public void ExportById(string name, int id, Action afterIntegrate = null) {
			ExportServiceEntity(name, id, afterIntegrate);
		}
		public abstract BaseServiceIntegrator CreateIntegrator();
		public abstract List<string> InitEntitiesName();
		public abstract List<string> InitServiceEntitiesName();
		public abstract void ImportBpmEntity(Entity entity);
		public void ExportAllServiceEntitiesByStep(int stepCount = 10, int rightLimit = 1000, Action afterIntegrate = null) {
			Limit = stepCount;
			Skip = 0;
			AfterIntegrate = afterIntegrate;
			foreach (var name in ServiceEntitiesName)
			{
				ClonsoleGreen("Start: " + name);
				try
				{
					for(Skip = 0; Skip - Limit < rightLimit; Skip += stepCount) {
						ExportServiceEntity(name);
					}
				}
				catch (Exception e)
				{
					ClonsoleGreen("Error: " + e.Message);
				}
				ClonsoleGreen("End: " + name);
			}
		}
		private void ClonsoleGreen(string text)
		{
			//var buff = Console.ForegroundColor;
			//Console.ForegroundColor = ConsoleColor.Green;
			//Console.WriteLine(text);
			//Console.ForegroundColor = buff;
		}

		private void Clonsole(string text)
		{
			//Console.WriteLine(text);
		}
	}
}
