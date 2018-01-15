#tool "nuget:?package=NUnit.ConsoleRunner"

Task("RunTest")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll");
	});

Task("BuildTestDashboards")
	//.IsDependentOn("RunTest")
	.Does(() => {
		StartProcess(@"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe",
			@"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\");
	});

Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.Does(() => {});

RunTarget("Build");