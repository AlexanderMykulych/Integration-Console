using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IStepBuilder
	{
		IStepBuilder OnDo(string eventName, string stepName, Action<IStepBuilder> currentStep = null);
		
		IStepBuilder InitCurrentStepName(string stepName);
		string GetCurrentStepName();
		Dictionary<string, List<EventStep>> GetEventSteps();
	}
}
