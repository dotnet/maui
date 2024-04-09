string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";

if (string.Equals(TARGET, "uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}

if (string.Equals(TARGET, "uitest-build", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}

if (string.Equals(TARGET, "cg-uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Compatibility/ControlGallery/test/iOS.UITests/Compatibility.ControlGallery.iOS.UITests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Compatibility/ControlGallery/src/iOS/Compatibility.ControlGallery.iOS.csproj";
}

void FailRunOnOnlyInconclusiveTests(string testResultsFile)
{
    // When all tests are inconclusive the run does not fail, check if this is the case and fail the pipeline so we get notified
	var totalTestCount = XmlPeek(testResultsFile, "/test-run/@total");
	var inconclusiveTestCount = XmlPeek(testResultsFile, "/test-run/@inconclusive");
	var errorMessage = string.Empty;

	// Try to get out error message from test results file, there is usually an error in there when tests are inconclusive
	try
	{
		errorMessage = XmlPeek(testResultsFile, "(//test-suite[@result='Inconclusive']/reason/message)[1]");
	}
	catch
	{
		// Intentionally left blank
	}

	if (totalTestCount.Equals(inconclusiveTestCount))
	{
		var exceptionMessage = "All tests are marked inconclusive, no tests ran. There is probably something wrong with running the tests.";

		if (!string.IsNullOrWhiteSpace(errorMessage))
		{
			exceptionMessage = $"{exceptionMessage} Error message extracted from {testResultsFile}, check this file for more details: {errorMessage}";
		}

		throw new Exception(exceptionMessage);
	}
}
