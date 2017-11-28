using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace QueryConsole.IntegrationJson {

	#region Class: JsonFilterProvider
	/*
		Project Path: ..\..\..\QueryConsole\IntegrationJson\JsonFilterProvider.cs
		
	*/
	public class JsonFilterProvider
	{
		public void Test(string tag)
		{
			using(var stream = new StreamReader(@"C:\VS_Project\IntegrationConsole\Integration-Console\Terra-integration\QueryConsole\IntegrationJson\IntegrationServiceAgent.json"))
			{
				var jsonText = stream.ReadToEnd();
				var json = JToken.Parse(jsonText);
				var result = json.SelectToken("data").Where(x => x.SelectToken("Agent.agentTags").Any(y => y.SelectToken("AgentTag.name").Value<string>() == tag)).Select(x => x.SelectToken("Agent.name").Value<string>() + " " + x.SelectToken("Agent.id").Value<string>());
				var text = "name id\n" + result.Aggregate((x, y) => x + "\n" + y);
				using(var streamWriter = new StreamWriter(string.Format(@"C:\VS_Project\IntegrationConsole\Integration-Console\Terra-integration\QueryConsole\IntegrationJson\IntegrationServiceAgent{0}.txt", tag)))
				{
					streamWriter.Write(text);
				}
			}
		}
	}

	#endregion

}