using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface ISettingProvider
	{
		void Init();
		void Reinit();
		ISetting Get(string settingName);
	}
}
