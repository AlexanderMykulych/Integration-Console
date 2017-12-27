using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework.Internal;
using Terrasoft.Core.Entities;
using NUnit.Framework;
using Terrasoft.Core.Factories;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.IntegrationSystem.Remedy
{
	[TestFixture]
	public class RemedyServiceTest
	{
		[Test]
		public void Request_CreateIncident_RemedyId()
		{
			var incident = CreateTestIncident();
			var remedyId = SendCreateIncidentRequest(incident);

			Assert.IsNotNull(remedyId);
			Assert.IsNotEmpty(remedyId);
		}

		private string SendCreateIncidentRequest(Entity incident)
		{
			var mock = new RemedyMockIntegrationConfig
			{
				PrepareServiceAction = config =>
				{
					if (config.Id == "Create_IncidentConfigService")
					{
						config.Url = @"http://localhost:1234/remedy/create_incident";
					}
					return config;
				}
			};
			ObjectFactory.CurrentKernel.Rebind<IIntegrationConfig>().ToConstant(mock);

			var integrator = ObjectFactory.Get<IIntegrator>();
			ConnectionProvider.DoWith(Setuper.userConnection,
				() =>
				{
					integrator.ExportWithRequest(incident.PrimaryColumnValue, incident.SchemaName,
						"Create_Case");
				});
			
			incident.FetchFromDB(incident.PrimaryColumnValue, false);
			return incident.GetTypedColumnValue<string>("TsiRemedyId");
		}

		private Entity CreateTestIncident()
		{
			var entity = GenerateCaseEntity();
			var insert = entity.CreateInsert();
			insert.Execute();
			return entity;
		}

		private Entity GenerateCaseEntity()
		{
			var userConnection = Setuper.userConnection;
			var schema = userConnection.EntitySchemaManager.GetInstanceByName("Case");
			var entity = schema.CreateEntity(userConnection);
			entity.SetDefColumnValues();
			return entity;
		}
	}
}
