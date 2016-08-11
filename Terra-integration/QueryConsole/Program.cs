﻿using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Terrasoft.Common;
using Terrasoft.TsConfiguration;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Terrasoft.Core.Factories;

namespace QueryConsole
{

	public class Program
	{
		public static void AddBindings() {
			//ClassFactory.ReBind<Terrasoft.Configuration.Passport.OrderPassportHelper, Terrasoft.Configuration.Passport.OrderPassportHelper>("SomeBind");
			//ClassFactory.Get<Terrasoft.Configuration.Passport.OrderPassportHelper>();
		}
		public static void Main(string[] args) {
			try {
				var date = Convert.ToDateTime("2016-08-11T06:58:59.673+0000");
				//var consoleApp = new TerrasoftConsoleClass("A.Mykulych");
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
				var testers = new List<BaseIntegratorTester>() {
					new OrderServiceIntegratorTester(consoleApp.SystemUserConnection),
					new ClientServiceIntegratorTester(consoleApp.SystemUserConnection)
				};
				//var testerManager = new TesterManager(consoleApp.SystemUserConnection, testers[0], testers[1]) {
				//	//{"ManagerInfo", 500, 0, 1},
				//	//{"CounteragentContactInfo", 500, 0, 1},
				//	//{"Counteragent", 500, 0, 1},
				//	//{"Contract", 500, 0, 1},
				//	//{"Order", 500, 0, 1},
				//	{"Shipment", 5, 0, 1},
				//	//{"Payment", 500, 0, 1},
				//	//{"Return", 500, 0, 1},
				//};
				//testerManager.Run();
				testers[1].ImportAllBpmEntity();
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
			var appSettings = (AppConfigurationSectionGroup) appConfigurationSectionGroup;
			string appDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);
			appSettings.Initialize(appDirectory, Path.Combine(appDirectory, "App_Data"), Path.Combine(appDirectory, "Resources"),
				appDirectory);
			AppConnection.Initialize(appSettings);
			AppConnection.InitializeWorkspace(WorkspaceName);
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
