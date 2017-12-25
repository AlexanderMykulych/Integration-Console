using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Instance
{
	public class BaseStrategyImplementation<T>: IStrategyImplementation<T>
	{
		private readonly Dictionary<string, List<EventStep>> _eventSteps;
		private readonly Dictionary<string, IStrategyStepInfo> _steps;
		private readonly string _initStepName;
		private string _currentStep;

		public BaseStrategyImplementation(Dictionary<string, IStrategyStepInfo> steps, Dictionary<string, List<EventStep>> eventSteps, string initStepName)
		{
			_steps = steps;
			_eventSteps = eventSteps;
			_initStepName = initStepName;
		}

		public void Execute(T input)
		{
			_currentStep = _initStepName;
			ExecuteStepWithParam(_currentStep, input);
		}

		public void ExecuteStepWithParam(string stepName, object input)
		{
			var step = GetStep(stepName);
			if (step != null)
			{
				SubscribeOnStepEvent(step, stepName);
				step.Execute(input);
			}
		}

		protected virtual IStrategyStep GetStep(string stepName)
		{
			if (_steps.ContainsKey(stepName))
			{
				var stepDescription = _steps[stepName];
				return stepDescription.FactoryFunc();
			}
			return null;
		} 
		private void SubscribeOnStepEvent(IStrategyStep step, string stepName)
		{
			if (_eventSteps.ContainsKey(stepName))
			{
				var events = _eventSteps[stepName];
				foreach (var eventStep in events)
				{
					step.On(eventStep.EventName, x => ExecuteStepWithParam(eventStep.StepName, x));
				}
			}
		}
	}
}
