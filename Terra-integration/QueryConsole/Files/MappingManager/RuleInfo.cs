using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
	public class RuleInfo
	{
		public MappingItem config;
		public Entity entity;
		public JToken json;
		public UserConnection userConnection;
		public CsConstant.TIntegrationType integrationType;
		public string action;
	}
}
