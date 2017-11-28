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
			var obj = JsonConvert.DeserializeObject<MappingConfig>(@"{""columns"":[{""name"":""TsiNumber"",""value"":""J00001357""},{""name"":""TsiSubject"",""value"":""Фінансові питання\\ Інформація по рахунку""},{""name"":""TsiRegisteredOn"",""value"":""2017-11-07T14:40:38.000Z""},{""name"":""TsiSolutionDate"",""value"":""2017-11-07T14:40:38.000Z""},{""name"":""TsiServiceCallCaseCategory"",""value"":""1c32801b-1c91-473f-959d-60e0fd0ffce8""},{""name"":""TsiProduct"",""value"":""58fbeb24-8e2e-17c0-e053-710c000ab07e""},{""name"":""TsiRote"",""value"":""e9177e07-10cc-43d2-ac16-950851d22fad""},{""name"":""TsiSupportGroup""},{""name"":""TsiSysAdminUnit""},{""name"":""TsiCaseDirection"",""value"":""571dd0c5-fc36-7cb2-e053-710c000a9ca7""},{""name"":""TsiCaseInitiatorType"",""value"":""571DD0C5-FC35-7CB2-E053-710C000A9CA7""},{""name"":""TsiAdmissionChannel"",""value"":""571dd0c5-fc39-7cb2-e053-710c000a9ca7""},{""name"":""TsiAccount"",""value"":""f6773221-c978-4807-b0de-c2c0baf79a8f""},{""name"":""TsiPersonalAccount"",""value"":""58219486-af2e-7479-e053-710c000a4670""},{""name"":""TsiDescription"",""value"":"""",""fileColumn"":""A""},{""name"":""TsiCaseState"",""value"":""5882D96B-DA7F-211C-E053-710C000A127D""},{""name"":""TsiContact""},{""name"":""TsiContactPhoneNumber""},{""name"":""TsiParent""}],""characteristic"":[{""name"":""34e06763-4452-46ae-ac98-bbc74b531a78""},{""name"":""80f2a352-82ab-4d11-974c-5987839ce231"",""fileColumn"":""A""},{""name"":""f71607b5-8dfb-41f9-89c5-f9ae21773e4e"",""value"":""df41e658-e38f-4e82-bd2c-7ffe2b3a7280""},{""name"":""2dc110f6-dc60-4464-beb8-dd832e544233""},{""name"":""dfb6f7c5-c663-4459-ba14-c05600843685""},{""name"":""ec9ca538-e312-4689-bc50-13149df10dff"",""value"":""cbd23469-5fef-428d-a9c7-8c9718bf751c""}]}");
			Console.ReadKey();
		}
	}
}
