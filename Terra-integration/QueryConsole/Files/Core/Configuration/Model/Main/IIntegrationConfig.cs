using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IIntegrationConfig
	{
		object Get(string settingName);
		void Init(XDocument document);
	}
}
