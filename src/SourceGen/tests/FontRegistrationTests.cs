using Microsoft.Build.Utilities;
using Microsoft.Maui.TestUtils.SourceGen;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.SourceGen.Tests
{
	public class FontGenerationTests : BaseSourceGeneratorTests<FontConfigurationGenerator>
	{
		public FontGenerationTests(ITestOutputHelper output) : base(output)
		{
			Generator.AddReferences("Microsoft.Maui");

			Generator.AddMSBuildProperty("UseMaui", "true");
			Generator.AddMSBuildProperty("OutputType", "Exe");
		}

		[Theory]
		[InlineData("ios", "TestFont", ".ttf", "Test")]
		[InlineData("maccatalyst", "TestFont", ".ttf", "Test")]
		[InlineData("android", "TestFont", ".ttf", "Test")]
		[InlineData("windows", "TestFont", ".ttf", "Test")]
		[InlineData("ios", "TestFont", ".otf", "Test")]
		[InlineData("maccatalyst", "TestFont", ".otf", "Test")]
		[InlineData("android", "TestFont", ".otf", "Test")]
		[InlineData("windows", "TestFont", ".otf", "Test")]
		public void AutoConfigure_Font_With_Alias(string platform, string font, string ext, string alias)
		{
			Generator.AddMSBuildProperty("TargetPlatformIdentifier", platform);

			Generator.AddMSBuildItems(new TaskItem(font + ext, new Dictionary<string, string>
			{
				{ "FontAlias", alias }
			}));

			RunGenerator();
			Compilation.AssertGeneratedContent($"fonts.AddFont(\"{font}{ext}\", \"{alias}\");");
		}

		[Theory]
		[InlineData("ios", "TestFont.ttf")]
		[InlineData("maccatalyst", "TestFont.ttf")]
		[InlineData("android", "TestFont.ttf")]
		[InlineData("windows", "TestFont.ttf")]
		public void AutoConfigure_Font_Without_Alias(string platform, string font)
		{
			Generator.AddMSBuildProperty("TargetPlatformIdentifier", platform);

			Generator.AddMSBuildItems(new TaskItem(font));

			RunGenerator();
			Compilation.AssertGeneratedContent("fonts.AddFont(\"TestFont.ttf\", null);");
		}
	}
}
