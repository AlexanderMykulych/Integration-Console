using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terrasoft.Core;
using Terrasoft.Configuration;

namespace QueryConsole.Files.IntegratorTester.PrimaryImport {

	#region Class: ImportServiceObject
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorTester\PrimaryImport\ImportServiceObject.cs
		
	*/
	public class ImportServiceObject
	{
		public void Run(UserConnection userConnection)
		{
			var options = new PrimaryImportParam("Counteragent", userConnection, true, 4841, 100, true, 0);
			var importProvider = new PrimaryImportProvider(options);
			importProvider.Run();
		}
		public void RunPrimary(UserConnection userConnection)
		{
			var scenario = new PrimaryImportScenario(userConnection, "ManagerInfo", 0, true, 100,
				"ManagerInfo", "CounteragentContactInfo", "Counteragent", "Contract", "ContractBalance", "Order", "Debt", "Shipment", "Payment", "Return");
			scenario
				.AddFilters("Counteragent", "q[active]=true")
				.AddFilters("Contract", "q[active]=true");
			scenario.Run();
		}
	}

	#endregion

}