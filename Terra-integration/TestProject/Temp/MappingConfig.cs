using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Terrasoft.Configuration.FileImport
{
	public class MappingConfig
	{
		[JsonProperty(PropertyName = "columns")]
		public List<MappingColumnsConfig> Columns;
		[JsonProperty(PropertyName = "characteristic")]
		public List<MappingCharacteristicConfig> Characteristic;
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
	public class MappingCharacteristicConfig: MappingColumnsConfig
	{
		[JsonProperty(PropertyName = "charTypeId")]
		public Guid CharTypeId;
		[JsonProperty(PropertyName = "charTypeName")]
		public string CharTypeName;
		[JsonProperty(PropertyName = "lookupName")]
		public string LookupName;
		[JsonProperty(PropertyName = "characterId")]
		public Guid CharacterId;
	}
}
