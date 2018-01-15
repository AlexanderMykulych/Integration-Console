using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Nancy;
using Nancy.Extensions;
using Terrasoft.TsIntegration.Configuration;

namespace IntegrationUnitTest.IntegrationSystem.Remedy
{
	public class RemedyService: NancyModule
	{
		public RemedyService()
		{
			Post["/remedy/create_incident"] = p =>
			{
				return @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:EFFTS_IWA_integrationWebAgent_2"">
				<soapenv:Header/>
					<soapenv:Body>
						<urn:Create_IncidentResponse>
							<urn:TextMessage>?</urn:TextMessage>
							<urn:Error>?</urn:Error>
							<urn:IncidentNumber>INC123456</urn:IncidentNumber>
						</urn:Create_IncidentResponse>
					</soapenv:Body>
				</soapenv:Envelope>";
			};

			Post["/remedy/create_incident_withoperatorcheck"] = p =>
			{
				var xElement = RemoveNamespaces(XElement.Parse(Request.Body.AsString()));
				var login = xElement.XPathSelectElement("//Remedy_Login_ID").Value;
				if (login != "WS_USER")
				{
					return
						@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
				   <soapenv:Body>
					  <ns0:Create_IncidentResponse xmlns:ns0=""EFFTS_IWA_integrationWebAgent_2"">
						 <ns0:TextMessage>Указанный пользователь не существует в ремеди</ns0:TextMessage>
						 <ns0:Error>10003</ns0:Error>
						 <ns0:IncidentNumber/>
					  </ns0:Create_IncidentResponse>
					</soapenv:Body>
					</soapenv:Envelope>";
				}
				return @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:EFFTS_IWA_integrationWebAgent_2"">
				<soapenv:Header/>
					<soapenv:Body>
						<urn:Create_IncidentResponse>
							<urn:TextMessage>?</urn:TextMessage>
							<urn:Error>?</urn:Error>
							<urn:IncidentNumber>INC2222222</urn:IncidentNumber>
						</urn:Create_IncidentResponse>
					</soapenv:Body>
				</soapenv:Envelope>";
			};
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
