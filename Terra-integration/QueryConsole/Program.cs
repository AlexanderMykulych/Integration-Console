using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Terrasoft.Core.Factories;
using Terrasoft.TsConfiguration;
using NodaTime.TimeZones;
using NodaTime;
using System.Linq;
using QueryConsole.Files.IntegratorTester.PrimaryImport;
using QueryConsole.IntegrationJson;
using Terrasoft.Configuration;
using BaseIntegratorTester = Terrasoft.TsConfiguration.BaseIntegratorTester;
using IntegrationServiceIntegrator = Terrasoft.TsConfiguration.IntegrationServiceIntegrator;
using OrderServiceIntegratorTester = Terrasoft.TsConfiguration.OrderServiceIntegratorTester;

namespace QueryConsole
{

	public class Program
	{
		public static void AddBindings() {
			//ClassFactory.ReBind<Terrasoft.Configuration.Passport.OrderPassportHelper, Terrasoft.Configuration.Passport.OrderPassportHelper>("SomeBind");
			//ClassFactory.Get<Terrasoft.Configuration.Passport.OrderPassportHelper>();
		}
		public static void Main(string[] args) {
			try
			{
				
				var consoleApp = new TerrasoftConsoleClass("Default");
				try {
					consoleApp.Run();
				} catch(Exception e) {
					consoleApp.ConsoleColorWrite("Connect to Database: Failed", ConsoleColor.Red);
					Console.WriteLine(e.Message);
				}

				consoleApp.ConsoleColorWrite("Connect to Database: Success");
				AddBindings();
				Console.WriteLine("Press any button to start integrate");
				Console.ReadKey();
				Console.WriteLine("Start");
				//CsConstant.IsDebugMode = true;
				var testers = new List<BaseIntegratorTester>() {
					new OrderServiceIntegratorTester(consoleApp.SystemUserConnection),
					new ClientServiceIntegratorTester(consoleApp.SystemUserConnection)
				};
				double val = 190165.26;
				var val2 = (float)val;
				Console.WriteLine(val2);
				//var testerManager = new TesterManager(consoleApp.SystemUserConnection, testers[0], testers[1]) {
				//	{"ManagerInfo", 500, 0, 1},
				//	{"CounteragentContactInfo", 500, 0, 1},
				//	{"Counteragent", 500, 0, 1},
				//	{"Contract", 500, 0, 1},
				//	{"Order", 500, 0, 1},
				//	{"Shipment", 5, 0, 1},
				//	{"Payment", 500, 0, 1},
				//	{"Return", 500, 0, 1},
				//};
				//testerManager.Run();
				//testers[1].ImportAllBpmEntity();
				//var integrator = new IntegrationServiceIntegrator(consoleApp.SystemUserConnection);
				//integrator.IniciateLoadChanges();
				//var regionProvider = new DeliveryServiceRegionProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>());
				//var cityProvider = new DeliveryServiceSettlementProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>());
				//var countryProvider = new DeliveryServiceCountryProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>());
				//var areaProvider = new DeliveryServiceAreaProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>());
				//var addressProvider = new DeliveryServiceAddressProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>());
				//var resultAddress = addressProvider.GetLookupValues("Россия,Новосибирская область,Новосибирск,Советский район,район Академгородок,Полевая 12,12");
				//var streetProvider = new DeliveryServiceStreetProvider(consoleApp.SystemUserConnection, new Dictionary<string, string>() {
				//	{ "settlement", "26067" }
				//});
				//var resultStreet = streetProvider.GetLookupValues("П");
				//var resultRegion = regionProvider.GetLookupValues("М");
				//var resultCity = cityProvider.GetLookupValues("М");
				//var resultCountry = countryProvider.GetLookupValues("Р");
				//var resultArea = areaProvider.GetLookupValues("М");
				//var tester = new ImportServiceObject();
				//tester.Run(consoleApp.SystemUserConnection);
				var exportScenario = new PrimaryExportScenario(consoleApp.SystemUserConnection, true, 100);
				exportScenario.Run();
				while (true) {
				}
			} catch (ReflectionTypeLoadException e1) {
				Console.WriteLine(e1.Message);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Press eny key!");
			Console.ReadKey();
		}
	}

	
	public class TerrasoftConsoleClass
	{
		
		public TerrasoftConsoleClass(string workspaceName) {
			WorkspaceName = workspaceName;
			SystemUserConnection = AppConnection.SystemUserConnection;
			AppManagerProvider
			= _appManagerProvider ?? (_appManagerProvider = AppConnection.AppManagerProvider);
		}

		 

		
		public string WorkspaceName {
			get;
			set;
		}

		public UserConnection SystemUserConnection;

		private AppConnection _appConnection;

		public AppConnection AppConnection {
			get {
				if (_appConnection == null) {
					_appConnection = new AppConnection();
				}
				return _appConnection;
			}
			protected set {
				_appConnection = value;
			}
		}

		private ManagerProvider _appManagerProvider;

		public ManagerProvider AppManagerProvider;

		 

		
		protected virtual Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args) {
			string requestingAssemblyName = args.Name;
			var appUri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
			string host = appUri.Host;
			if (!string.IsNullOrEmpty(host)) {
				host = @"\\" + host;
			}
			string appPath = Path.Combine(host, Path.GetDirectoryName(Uri.UnescapeDataString(appUri.Path.TrimStart('/'))));
			var processRunMode = Environment.Is64BitProcess ? "x64" : "x86";
			int index = requestingAssemblyName.IndexOf(',');
			if (index > 0) {
				string requestingAssemblyPath = Path.Combine(appPath, processRunMode,
					requestingAssemblyName.Substring(0, index) + ".dll");
				if (System.IO.File.Exists(requestingAssemblyPath)) {
					return Assembly.LoadFrom(requestingAssemblyPath);
				}
			}
			return null;
		}

		protected AppConfigurationSectionGroup GetAppSettings() {
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = (AppConfigurationSectionGroup) configuration.SectionGroups["terrasoft"];
			appSettings.RootConfiguration = configuration;
			return appSettings;
		}

		protected virtual void Initialize(ConfigurationSectionGroup appConfigurationSectionGroup) {
			try {
				var appSettings = (AppConfigurationSectionGroup) appConfigurationSectionGroup;
				string appDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);
				appSettings.Initialize(appDirectory, Path.Combine(appDirectory, "App_Data"), Path.Combine(appDirectory, "Resources"),
					appDirectory);
				AppConnection.Initialize(appSettings);
				AppConnection.InitializeWorkspace(WorkspaceName);
			} catch(Exception e) {
				//Nothing
			}
		}

		 

		
		public void Run() {
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			AppConfigurationSectionGroup appSettings = GetAppSettings();
			var resources = (ResourceConfigurationSectionGroup) appSettings.SectionGroups["resources"];
			GeneralResourceStorage.Initialize(resources);
			Initialize(appSettings);
		}		
		public EntityCollection GetEntitiesForUpdate(string name, bool onlyNotImportet = false) {
			var esq = new EntitySchemaQuery(SystemUserConnection.EntitySchemaManager, name);
			esq.AddAllSchemaColumns();
			if(onlyNotImportet) {
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsExternalId", 0));
			}
			return esq.GetEntityCollection(SystemUserConnection);
		}

		
		public void ConsoleColorWrite(string text, ConsoleColor color = ConsoleColor.Green) {
			var buff = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = buff;
		}
		 
	}

}
