using System;
using System.IO;
using System.Reflection;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest
{
	[SetUpFixture]
	public class Setuper
	{
		public static UserConnection userConnection;
		[OneTimeSetUp]
		public void Setup()
		{
			var consoleApp = new TerrasoftConsoleClass("Default");
			consoleApp.Run();
			userConnection = consoleApp.SystemUserConnection;
		}
	}


	public class TerrasoftConsoleClass
	{

		public TerrasoftConsoleClass(string workspaceName)
		{
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
				if (_appConnection == null)
				{
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




		protected virtual Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string requestingAssemblyName = args.Name;
			var appUri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
			string host = appUri.Host;
			if (!string.IsNullOrEmpty(host))
			{
				host = @"\\" + host;
			}
			string appPath = Path.Combine(host, Path.GetDirectoryName(Uri.UnescapeDataString(appUri.Path.TrimStart('/'))));
			var processRunMode = Environment.Is64BitProcess ? "x64" : "x86";
			int index = requestingAssemblyName.IndexOf(',');
			if (index > 0)
			{
				string requestingAssemblyPath = Path.Combine(appPath, processRunMode,
					requestingAssemblyName.Substring(0, index) + ".dll");
				if (System.IO.File.Exists(requestingAssemblyPath))
				{
					return Assembly.LoadFrom(requestingAssemblyPath);
				}
			}
			return null;
		}

		protected AppConfigurationSectionGroup GetAppSettings()
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = (AppConfigurationSectionGroup)configuration.SectionGroups["terrasoft"];
			appSettings.RootConfiguration = configuration;
			return appSettings;
		}

		protected virtual void Initialize(ConfigurationSectionGroup appConfigurationSectionGroup)
		{
			try
			{
				var appSettings = (AppConfigurationSectionGroup)appConfigurationSectionGroup;
				string appDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);
				appSettings.Initialize(appDirectory, Path.Combine(appDirectory, "App_Data"), Path.Combine(appDirectory, "Resources"),
					appDirectory);
				AppConnection.Initialize(appSettings);
				AppConnection.InitializeWorkspace(WorkspaceName);
			}
			catch (Exception e)
			{
				//Nothing
			}
		}

		public void Run()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			AppConfigurationSectionGroup appSettings = GetAppSettings();
			var resources = (ResourceConfigurationSectionGroup)appSettings.SectionGroups["resources"];
			resources.GeneralSettingsSection.ResourceDirectory = "C:\\Dev\\R&D\\DynamicIntegration\\Integration-Console\\Terra-integration\\IntegrationUnitTest\\bin\\Debug\\Resources";
			GeneralResourceStorage.Initialize(resources);
			Initialize(appSettings);
		}
		public EntityCollection GetEntitiesForUpdate(string name, bool onlyNotImportet = false)
		{
			var esq = new EntitySchemaQuery(SystemUserConnection.EntitySchemaManager, name);
			esq.AddAllSchemaColumns();
			if (onlyNotImportet)
			{
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsExternalId", 0));
			}
			return esq.GetEntityCollection(SystemUserConnection);
		}


		public void ConsoleColorWrite(string text, ConsoleColor color = ConsoleColor.Green)
		{
			var buff = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = buff;
		}

	}

}
