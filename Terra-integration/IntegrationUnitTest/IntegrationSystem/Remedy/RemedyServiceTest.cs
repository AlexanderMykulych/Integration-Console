using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework.Internal;
using NUnit.Framework;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.IntegrationSystem.Remedy
{
	[TestFixture]
	[Category("Remedy")]
	public class RemedyServiceTest
	{
		private string _remedy_CreateIncident_Url;
		private string _remedy_CreateIncidentWithOperatorCheck_Url;

		public RemedyServiceTest()
		{
			_remedy_CreateIncident_Url = @"http://localhost:1234/remedy/create_incident";
			_remedy_CreateIncidentWithOperatorCheck_Url = @"http://localhost:1234/remedy/create_incident_withoperatorcheck";
		}
		[SetUp]
		public void SetUp()
		{
			ClearServiceSettings();
		}

		private void ClearServiceSettings()
		{
			//TestContext.CurrentContext;
		}

		[Test, Order(1)]
		[Benchmark()]
		public void Request_CreateIncident_RemedyId()
		{
			var incident = CreateTestIncident();
			var remedyId = SendCreateIncidentRequest(incident, _remedy_CreateIncident_Url);

			Assert.IsNotNull(remedyId);
			Assert.IsNotEmpty(remedyId);
			Assert.Pass("Id Remedy: '{0}'", remedyId);
		}

		[Test, Order(2)]
		[Benchmark]
		public void Request_CreateIncidentWithNotExistOperator_AddReupdateCaseToTriggerQueue()
		{
			var incident = CreateTestIncident();
			ConnectionProvider.DoWith(Setuper.userConnection, () =>
			{
				var setting = ObjectFactory.Get<ISettingProvider>();
				setting.Reinit();
			});
			var remedyId = SendCreateIncidentRequest(incident, _remedy_CreateIncidentWithOperatorCheck_Url);
			Assert.IsTrue(string.IsNullOrEmpty(remedyId), "Отправка с некоректным оператором " + remedyId.ToString());

			var isTriggerExist = GetIsCaseTriggerAdded(incident.PrimaryColumnValue, "AddCaseWithoutOperatorLogin");
			UpdateCaseTrigger(incident.PrimaryColumnValue, "AddCaseWithoutOperatorLogin");
			Assert.IsTrue(isTriggerExist, "Проверка существования тригера");
			remedyId = SendCreateIncidentRequest(incident, _remedy_CreateIncidentWithOperatorCheck_Url, GetRouteByTrigger("AddCaseWithoutOperatorLogin"));

			Assert.IsNotEmpty(remedyId, "Повторная отправка");
			Assert.Pass("Id Remedy: '{0}'", remedyId);
		}

		private bool GetIsCaseTriggerAdded(Guid id, string triggerName)
		{
			var userConnection = Setuper.userConnection;
			return (new Select(userConnection)
				.Column(Func.Count("Id"))
				.From("TsiTriggerQueue")
				.Where("TsiObjectName").IsEqual(Column.Parameter("Case"))
				.And("TsiObjectId").IsEqual(Column.Parameter(id))
				.And("TsiTriggerName").IsEqual(Column.Parameter(triggerName))
				.And("TsiState").IsEqual(Column.Parameter(0)) as Select)
			.ExecuteScalar<int>() > 0;
		}

		private void UpdateCaseTrigger(Guid id, string triggerName)
		{
			var userConnection = Setuper.userConnection;
			var update = new Update(userConnection, "TsiTriggerQueue")
				.Set("TsiState", Column.Parameter(1))
				.Where("TsiObjectName").IsEqual(Column.Parameter("Case"))
				.And("TsiObjectId").IsEqual(Column.Parameter(id))
				.And("TsiTriggerName").IsEqual(Column.Parameter(triggerName));
			update.Execute();
		}

		private string SendCreateIncidentRequest(Entity incident, string remedyUrl, string routeName = "Create_Case")
		{
			var mock = new RemedyMockIntegrationConfig
			{
				PrepareServiceAction = config =>
				{
					if (config.Id == "Create_IncidentConfigService")
					{
						config.Url = remedyUrl;
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
						routeName);
				});
			
			incident.FetchFromDB(incident.PrimaryColumnValue, false);
			return incident.GetTypedColumnValue<string>("TsiRemedyId");
		}

		private string GetRouteByTrigger(string triggerName)
		{
			var triggerEngine = ObjectFactory.Get<TriggerEngine>();
			return ConnectionProvider.DoWith(Setuper.userConnection,
				() => triggerEngine.GetTriggerByName(triggerName, Setuper.userConnection).Route, string.Empty);
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
