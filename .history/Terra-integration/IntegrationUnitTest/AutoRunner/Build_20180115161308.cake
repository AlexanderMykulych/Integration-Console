#tool "nuget:?package=NUnit.ConsoleRunner"

Task("RunTest")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll");
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll");
	});

Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.Does(() => {});

RunTarget("Run Test");