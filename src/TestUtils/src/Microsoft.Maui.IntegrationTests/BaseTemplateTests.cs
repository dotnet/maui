namespace Microsoft.Maui.IntegrationTests;

public abstract class BaseTemplateTests : BaseBuildTest
{
	protected BaseTemplateTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output)
	{
		// Constructor setup (equivalent to [SetUp])
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.props"),
			Path.Combine(TestDirectory, "Directory.Build.props"), true);
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.targets"),
			Path.Combine(TestDirectory, "Directory.Build.targets"), true);
	}

	protected void AssertContains(string expected, string actual)
	{
		Assert.True(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to contain '{expected}'.");
	}

	protected void AssertDoesNotContain(string expected, string actual)
	{
		Assert.False(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to not contain '{expected}'.");
	}
}
