using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Reflection;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Terrasoft.Configuration;
using Terrasoft.Core.Factories;
using Terrasoft.TsIntegration.Configuration;

namespace QueryConsole
{

	public class Program
	{
		public static void Main(string[] args) {
			try
			{

				var consoleApp = new TerrasoftConsoleClass("Default");
				try
				{
					consoleApp.Run();
				}
				catch (Exception e)
				{
					consoleApp.ConsoleColorWrite("Connect to Database: Failed", ConsoleColor.Red);
					Console.WriteLine(e.Message);
				}

				var userConnection = consoleApp.SystemUserConnection;
				var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Account");
				esq.AddColumn("Id");
				esq.AddColumn("Name");
				esq.AddColumn("Address");
				esq.AddColumn("City");
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Contain, "=[TsiPersonalAccount:TsiAccount:Id].=[TsiPersAccAddress:TsiPersonalAccount:Id].=[TsiAddress:Id:TsiAddress].TsiAddressName", "вул"));


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
