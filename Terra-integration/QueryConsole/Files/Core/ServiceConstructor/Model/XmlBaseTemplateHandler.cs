using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	[TemplateAttribute("XmlTemplateHandler")]
	public class XmlBaseTemplateHandler : ITemplateHandler
	{
		//Log key=XmlHandler, throw
		public virtual void Import(TemplateSetting templateSettings, IntegrationInfo integrationInfo)
		{
			string sysPrefixName = templateSettings.Settings["sysPrefixName"];
			var element = integrationInfo.Data.GetObject() as XElement;
			if (element != null)
			{
				element = RemoveNamespaces(element);
				var bodyElement = element.Descendants().Where(x => x.Name.LocalName == "Body").FirstOrDefault();
				integrationInfo.Data = new IntegrXObject((XElement)bodyElement.FirstNode);
			}
		}
		//Log key=XmlHandler, throw
		public virtual void Export(TemplateSetting templateSettings, IntegrationInfo integrationInfo)
		{
			XNamespace soapPrefix = templateSettings.Settings["soapPrefix"];
			string sysPrefixName = templateSettings.Settings["sysPrefixName"];
			XNamespace sysPrefixValue = templateSettings.Settings["sysPrefixValue"];
			var headerNodes = GetHeaderNodes(templateSettings);
			var element = integrationInfo.Data.GetObject() as XElement;
			integrationInfo.Data = WrapElement(element, soapPrefix, sysPrefixName, sysPrefixValue, headerNodes);
		}
		//Log key=XmlHandler, throw
		protected virtual IIntegrationObject WrapElement(XElement element, XNamespace soapPrefix, string sysPrefixName, XNamespace sysPrefixValue, List<XElement> headers)
		{
			var wrappedElement = new XElement(soapPrefix + "Envelope",
				new XAttribute(XNamespace.Xmlns + "soapenv", soapPrefix),
				new XAttribute(XNamespace.Xmlns + sysPrefixName, sysPrefixValue),
				new XElement(soapPrefix + "Header",
					AddNamespaceToXElement(new XElement("AuthenticationInfo", headers), sysPrefixValue)),
				new XElement(soapPrefix + "Body", AddNamespaceToXElement(element, sysPrefixValue)));
			return new IntegrXObject(wrappedElement);
		}
		//Log key=XmlHandler, throw
		public List<XElement> GetHeaderNodes(TemplateSetting templateSettings)
		{
			if (templateSettings.Settings.ContainsKey("Headers"))
			{
				return templateSettings.Settings["Headers"]
					.Split(',')
					.Where(x => !string.IsNullOrEmpty(x) && templateSettings.Settings.ContainsKey(x))
					.Select(x => new XElement(x, templateSettings.Settings[x]))
					.ToList();
			}
			return new List<XElement>();
		}
		public XElement AddNamespaceToXElement(XElement xElement, XNamespace xNamespace)
		{
			foreach (XElement el in xElement.DescendantsAndSelf())
			{
				el.Name = xNamespace.GetName(el.Name.LocalName);
			}
			return xElement;
		}
		public XElement RemoveNamespaces(XElement root)
		{
			XElement res = new XElement(
				root.Name.LocalName,
				root.HasElements ?
					root.Elements().Select(el => RemoveNamespaces(el)) :
					(object)root.Value
			);
			res.ReplaceAttributes(
				root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));
			return res;
		}
	}
}