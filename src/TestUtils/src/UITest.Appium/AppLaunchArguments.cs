using System.Text.RegularExpressions;

namespace UITest.Appium;

public static class AppLaunchArguments
{
	public static string AndroidTestExtra(string testName) =>
		$"--es test {QuoteAndroidShellArgument(testName)}";

	public static string QuoteAndroidShellArgument(string value) =>
		"\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal)
			.Replace("$", "\\$", StringComparison.Ordinal)
			.Replace("`", "\\`", StringComparison.Ordinal) + "\"";

	public static string WindowsTestArgument(string testName)
	{
		var argument = "--test=" + testName;
		argument = Regex.Replace(argument, "(\\\\*)\"", "$1$1\\\"");
		argument = Regex.Replace(argument, "(\\\\+)$", "$1$1");
		return "\"" + argument + "\"";
	}
}
