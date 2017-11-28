using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Infrastructure.Language;
using Terrasoft.Common;

namespace Terrasoft.TsIntegration.Configuration
{
	public abstract class BaseAttributeFactory<TResult, TAttribute, TName> : IFactory<TResult, TName>
	{
		protected abstract Dictionary<TAttribute, TResult> Instances { get; }
		protected abstract bool IsRuleRegister { get; set; }
		public virtual TResult Get(TName name)
		{
			RegisterRules();
			return Instances.First(x => IsInstanceNameEqual(x, name)).Value;
		}

		protected abstract bool IsInstanceNameEqual(KeyValuePair<TAttribute, TResult> instanceKeyValue, TName name);

		protected virtual void RegisterRules()
		{
			if (IsRuleRegister)
			{
				return;
			}
			var assembly = this.GetType().Assembly;
			var ruleAttrType = typeof(TAttribute);
			assembly
				.GetTypes()
				.Where(x => x.HasAttribute(ruleAttrType))
				.ForEach(x =>
				{
					var attributess = x.GetCustomAttributes(ruleAttrType, true);
					if (attributess.Length == 0)
					{
						return;
					}
					attributess
						.Where(attr => attr is TAttribute)
						.ForEach(attr => RegisterRule((TAttribute)attr, CreateRuleInstanse(x)));
				});
			IsRuleRegister = true;
		}

		protected virtual void RegisterRule(TAttribute attr, TResult instance)
		{
			if (!Instances.ContainsKey(attr))
			{
				Instances.Add(attr, instance);
			}
		}

		protected virtual TResult CreateRuleInstanse(Type ruleType)
		{
			return (TResult)Activator.CreateInstance(ruleType);
		}
	}
}
