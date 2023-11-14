string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";

if (string.Equals(TARGET, "uitest", StringComparison.OrdinalIgnoreCase))
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
	
	if (totalTestCount.Equals(inconclusiveTestCount))
    {
		throw new Exception("All tests are marked inconclusive, no tests ran. There is probably something wrong with running the tests.");
	}
}
