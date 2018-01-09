using System.Text;
using System;
namespace Terrasoft.TsIntegration.Configuration{
	public class TestScenarioProvider
	{
		public StringBuilder LogBuilder;
		public int Count;
		public bool Enable;
		public TestScenarioProvider()
		{
			LogBuilder = new StringBuilder();
			Count = 0;
			Enable = true;
			LogBuilder.AppendLine("--Начало--");
		}
		public TestScenarioProvider Do(string name, Func<string> actionPredicate, bool stopIfError = true)
		{
			if (!Enable)
			{
				return this;
			}
			try
			{
				LogBuilder.AppendLineFormat("{0}. {1}", ++Count, name);
				var str = actionPredicate();
				if (!string.IsNullOrEmpty(str))
				{
					LogBuilder.AppendLine(str);
				}
			}
			catch (Exception e)
			{
				LogBuilder.AppendLineFormat("Ошибка: {0}", e);
				if (stopIfError)
				{
					Enable = false;
				}
			}
			return this;
		}

		public string End()
		{
			LogBuilder.AppendLine("--Конец--");
			return LogBuilder.ToString();
		}
	}
}