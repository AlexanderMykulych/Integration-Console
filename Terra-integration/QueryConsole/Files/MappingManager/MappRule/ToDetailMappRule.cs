﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files.MappingManager.MappRule
{
	class ToDetailMappRule : BaseMappRule
	{
		public ToDetailMappRule()
		{
			_type = "todetail";
		}
		public override void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				object resultValue = null;
				var newValue = JsonEntityHelper.GetSimpleTypeValue(info.json);
				if (newValue != null && !string.IsNullOrEmpty(newValue.ToString()))
				{
					resultId = info.entity.GetColumnValue(info.config.TsSourcePath);
					var optionalColumns = new List<Tuple<string, string>>();
					if (!string.IsNullOrEmpty(info.config.TsDetailTag)) {
						optionalColumns = ParseDetailTag(info.config.TsDetailTag).ToList();
					}
					optionalColumns.Add(new Tuple<string, string>(info.config.TsDetailPath, resultId.ToString()));
					if (info.config.TsTag == "simple")
					{
						resultValue = newValue.ToString();
					}
					else if (info.config.TsTag == "stringtoguid")
					{
						resultValue = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
					}
					var filters = new List<Tuple<string, object>>() {
						new Tuple<string, object>(info.config.TsDetailPath, resultId)
					};
					JsonEntityHelper.UpdateOrInsertEntityColumn(info.config.TsDetailName, info.config.TsDetailResPath, resultValue, info.userConnection, optionalColumns, filters);
				}
			}
		}
		public override void Export(RuleExportInfo info)
		{
			object resultObject = null;
			var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
			var detailValue = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDetailName, info.config.TsDetailPath, sourceValue, info.config.TsDetailResPath).FirstOrDefault();
			if (info.config.TsTag == "simple")
			{
				resultObject = detailValue;
			}
			else if (info.config.TsTag == "stringtoguid")
			{
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, detailValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json = resultObject != null ? JToken.FromObject(resultObject) : null;
		}

		public IEnumerable<Tuple<string, string>> ParseDetailTag(string tag)
		{
			return tag.Split(',').Select(x =>
			{
				var block = x.Split('|');
				return new Tuple<string, string>(block[0], block[1]);
			});
		}
	}
}