using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace TestProject
{
	public class MappingConfig
	{
		[JsonProperty(PropertyName = "columns")]
		public List<MappingColumnsConfig> Columns;
		[JsonProperty(PropertyName = "characteristic")]
		public List<MappingColumnsConfig> Characteristic;
	}
	public class MappingColumnsConfig
	{
		[JsonProperty(PropertyName = "name")]
		public string Name;
		[JsonProperty(PropertyName = "value")]
		public string Value;
		[JsonProperty(PropertyName = "fileColumn")]
		public string FileColumn;
	}
	class Program
	{
		static void Main(string[] args)
		{
			string path = "";
			foreach (var chromeProcess in Process.GetProcessesByName("chrome"))
			{
				var process = chromeProcess.Modules[0];	
				path = process.FileName;
				chromeProcess.Kill();
			}
			Thread.Sleep(2000);
			if (!string.IsNullOrEmpty(path))
			{
				Process.Start(path, "-enable-usermedia-screen-capturing");
			}
			else
			{
				Console.WriteLine("Active Chrome not found!");
				Console.ReadKey();
			}
		}
	}
}
