#tool "nuget:?package=NUnit.ConsoleRunner"
using Cake.Email.Common;
var mySetting = Argument("testDll", "../bin/Debug/IntegrationUnitTest.dll");
Task("RunTest")
	.Does(() => {
		NUnit3(testDll);
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		StartProcess(@"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe",
			@"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\");
	});
	
Task("SendResultEmail")
	.Does(() => {
		var path = @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\";
		var apiKey = "";
		var attachments = new[]
		{
			Attachment.FromLocalFile(path)
		};
		try
		{
			var result = SendGrid.SendEmail(
				senderName: "Test System",
				senderAddress: "alexander.mykulych@gmail.com",
				recipient: new Cake.Email.Common.MailAddress(@"a.mykulych@tsintegr.com", "Alexander Mykulych"),
				subject: "This is a test",
				htmlContent: "This is a test",
				textContent: "This is a test",
				attachments: attachments,
				settings: new SendGridEmailSettings { ApiKey = apiKey }
			);
			if (result.Ok)
			{
				Information("Email succcessfully sent");
			}
			else
			{
				Error("Failed to send email: {0}", result.Error);
			}
		}
		catch(Exception ex)
		{
			Error("{0}", ex);
		}
	});

Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.IsDependentOn("SendResultEmail")
	.Does(() => {});

RunTarget("Build");