#tool "nuget:?package=NUnit.ConsoleRunner"

Task("Run Test")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll");
	});

RunTarget("Run Test");