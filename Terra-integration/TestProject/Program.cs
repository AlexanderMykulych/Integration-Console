using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			var str = "	\t<soapenv>\r\n\t\t<Create_IncidentResponse>\r\n\t\t\t<TextMessage></TextMessage>\r\n\t\t\t<Error>100003</Error>\r\n\t\t\t<IncidentNumber/>\r\n\t\t</Create_IncidentResponse>\r\n\t</soapenv>";
			var el = XElement.Parse(str);
			var doc = new XDocument(el);
			var res = doc.XPathEvaluate("/Create_IncidentResponse");
			Console.ReadKey();
		}
	}
}
