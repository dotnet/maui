using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class FontTargetDependencyTests : BaseTest
	{
		public FontTargetDependencyTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData("android", true)]
		[InlineData("ios", false)]
		[InlineData("windows", false)]
		[InlineData("tizen", true)]
		public void ProcessMauiFonts_Should_Not_Depend_On_ResizetizeCollectItems_For_AfterTargets_Platforms(string platform, bool shouldRemoveDependency)
		{
			// This test validates the fix for issue #23268 where ProcessMauiFonts had both
			// a dependency on ResizetizeCollectItems AND ran AfterTargets ResizetizeCollectItems
			// which created a race condition on first build.

			var targetsFile = Path.Combine(
				Directory.GetCurrentDirectory(),
				"../../../../../src/SingleProject/Resizetizer/src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets");

			Assert.True(File.Exists(targetsFile), $"Targets file not found: {targetsFile}");

			var content = File.ReadAllText(targetsFile);

			// Find the platform-specific PropertyGroup that overrides ProcessMauiFontsDependsOnTargets
			var overrideSectionStart = content.IndexOf("<!-- Override ProcessMauiFontsDependsOnTargets for platforms that use AfterTargets=\"ResizetizeCollectItems\" -->");
			Assert.True(overrideSectionStart > 0, "Override section not found in targets file");

			// Find the condition for the override
			var conditionStart = content.IndexOf("Condition=\"'$(_ResizetizerIsAndroidApp)' == 'True' Or '$(_ResizetizerIsTizenApp)' == 'True'\"", overrideSectionStart);
			Assert.True(conditionStart > 0, "Override condition not found");

			// Find the ProcessMauiFontsDependsOnTargets property in the override section
			var overrideSectionEnd = content.IndexOf("</PropertyGroup>", conditionStart);
			var overrideSection = content.Substring(conditionStart, overrideSectionEnd - conditionStart);

			if (shouldRemoveDependency)
			{
				// For Android and Tizen, the override should NOT include ResizetizeCollectItems
				Assert.DoesNotContain("ResizetizeCollectItems", overrideSection, StringComparison.Ordinal);
				Assert.Contains("ProcessMauiAssets", overrideSection, StringComparison.Ordinal);
				Assert.Contains("ProcessMauiSplashScreens", overrideSection, StringComparison.Ordinal);
			}

			// Also check that the platform-specific sections still have AfterTargets for the problematic platforms
			if (platform == "android")
			{
				Assert.Contains("ProcessMauiFontsAfterTargets", content, StringComparison.Ordinal);
				var androidSection = content.Substring(content.IndexOf("<!-- Android -->", StringComparison.Ordinal));
				var androidSectionEnd = content.IndexOf("<!-- Windows App SDK -->", androidSection.IndexOf("<!-- Android -->", StringComparison.Ordinal) + 1, StringComparison.Ordinal);
				var androidContent = androidSection.Substring(0, androidSectionEnd - androidSection.IndexOf("<!-- Android -->", StringComparison.Ordinal));
				Assert.Contains("ResizetizeCollectItems", androidContent, StringComparison.Ordinal);
			}
			else if (platform == "tizen")
			{
				Assert.Contains("ProcessMauiFontsAfterTargets", content, StringComparison.Ordinal);
				var tizenSection = content.Substring(content.IndexOf("<!-- Tizen -->", StringComparison.Ordinal));
				Assert.Contains("ResizetizeCollectItems", tizenSection, StringComparison.Ordinal);
			}
		}

		[Fact]
		public void ProcessMauiFonts_Target_Should_Have_Correct_Dependencies()
		{
			// Verify that the ProcessMauiFonts target itself references the correct DependsOnTargets property
			var targetsFile = Path.Combine(
				Directory.GetCurrentDirectory(),
				"../../../../../src/SingleProject/Resizetizer/src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets");

			Assert.True(File.Exists(targetsFile), $"Targets file not found: {targetsFile}");

			var content = File.ReadAllText(targetsFile);

			// Find the ProcessMauiFonts target definition
			var targetStart = content.IndexOf("<Target Name=\"ProcessMauiFonts\"");
			Assert.True(targetStart > 0, "ProcessMauiFonts target not found");

			var targetEnd = content.IndexOf("</Target>", targetStart);
			var targetContent = content.Substring(targetStart, targetEnd - targetStart);

			// Verify it uses the DependsOnTargets property
			Assert.Contains("DependsOnTargets=\"$(ProcessMauiFontsDependsOnTargets)\"", targetContent, StringComparison.Ordinal);
		}
	}
}