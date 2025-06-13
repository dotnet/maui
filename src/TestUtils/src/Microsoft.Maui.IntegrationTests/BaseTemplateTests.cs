using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests;

public abstract class BaseTemplateTests : BaseBuildTest
{
	protected BaseTemplateTests() : base()
	{
		// Constructor performs setup that was previously in [SetUp] method
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
		Assert.Contains(expected, actual, StringComparison.Ordinal);
	}

	protected void AssertDoesNotContain(string expected, string actual)
	{
		Assert.DoesNotContain(expected, actual, StringComparison.Ordinal);
	}
}
