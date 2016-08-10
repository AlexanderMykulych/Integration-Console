﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using QueryConsole.Files.Extension;

namespace QueryConsole.Files.XmlManager {
	public static class DynamicXmlParser {
		public static T StartMapXmlToObj<T>(XmlNode node, Type objType, object defObj = null, Func<string, string> prepareValuePredicate = null) {
			object resultObj = null;
			if (defObj == null) {
				resultObj = Activator.CreateInstance(objType);
			} else {
				resultObj = defObj.CloneObject();
			}
			var columnsName = objType.GetProperties().Where(x => x.MemberType == MemberTypes.Property).Select(x => x.Name).ToList();
			foreach (var columnName in columnsName) {
				PropertyInfo propertyInfo = objType.GetProperty(columnName);
				var xmlAttr = node.Attributes[columnName];
				if (xmlAttr != null) {
					var value = xmlAttr.Value;
					if (prepareValuePredicate != null) {
						value = prepareValuePredicate(value);
					}
					var propertyType = propertyInfo.PropertyType;
					if (propertyType.IsEnum || propertyType == typeof(int)) {
						propertyInfo.SetValue(resultObj, int.Parse(value));
					} else if (propertyType == typeof(bool)) {
						propertyInfo.SetValue(resultObj, value != "0");
					} else {
						propertyInfo.SetValue(resultObj, value);
					}
				}
			}
			if(resultObj is T) {
				return (T)resultObj;
			}
			return default(T);
		}
	}
}