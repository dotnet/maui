namespace Microsoft.Maui.IntegrationTests;

public abstract class BaseTemplateTests : BaseBuildTest
{
	[SetUp]
	public void TemplateTestsSetUp()
	{
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.props"),
			Path.Combine(TestDirectory, "Directory.Build.props"), true);
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.targets"),
			Path.Combine(TestDirectory, "Directory.Build.targets"), true);
	}

	protected void EnableTizen(string projectFile)
	{
		FileUtilities.ReplaceInFile(projectFile, new Dictionary<string, string>()
		{
			{ "<!-- <TargetFrameworks>", "<TargetFrameworks>" },
			{ "</TargetFrameworks> -->", "</TargetFrameworks>" },
		});
	}

	protected void AssertContains(string expected, string actual)
	{
		Assert.IsTrue(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to contain '{expected}'.");
	}

	protected void AssertDoesNotContain(string expected, string actual)
	{
		Assert.IsFalse(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to not contain '{expected}'.");
	}
}
