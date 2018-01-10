using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public static class BuilderStepExtensions
	{
		public static IStepBuilder OnSuccessDo(this IStepBuilder step, string stepName, Action<IStepBuilder> currentStep = null)
		{
			return step.OnDo("Success", stepName, currentStep);
		}
		public static IStepBuilder OnErrorDo(this IStepBuilder step, string stepName, Action<IStepBuilder> currentStep = null)
		{
			return step.OnDo("Error", stepName, currentStep);
		}

		public static IStepBuilder OnDo(this IStepBuilder step, List<string> eventsName, string stepName,
			Action<IStepBuilder> currentStep = null)
		{
			foreach (var eventName in eventsName)
			{
				step.OnDo(eventName, stepName, currentStep);
			}
			return step;
		}
	}
}
