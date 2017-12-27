using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

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
		}
	}
}
