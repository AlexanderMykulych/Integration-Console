using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Terrasoft.TsIntegration.Configuration
{
	public class StringToIObjectStep : BaseStrategyStep
	{
		private readonly TIntegrationObjectType _objectType;

		public StringToIObjectStep(TIntegrationObjectType objectType)
		{
			_objectType = objectType;
		}
		public override void Execute(object input)
		{
			var text = (string)input;
			IIntegrationObject result = null;
			switch (_objectType)
			{
				case TIntegrationObjectType.Json:
					result = new IntegrJObject(JObject.Parse(text));
					break;
				case TIntegrationObjectType.Xml:
					result = new IntegrXObject(XElement.Parse(text));
					break;
				default:
					Subscriber.Execute("Error");
					break;
			}
			Subscriber.Execute("Success", result);
		}
	}
}
