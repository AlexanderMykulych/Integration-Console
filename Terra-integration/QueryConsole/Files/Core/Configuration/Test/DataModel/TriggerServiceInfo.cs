using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[DataContract]
	public class TriggerServiceInfo
	{
		[DataMember]
		public string TriggerName;
		[DataMember]
		public string SchemaName;
		[DataMember]
		public Guid PrimaryColumnValue;
	}
}
