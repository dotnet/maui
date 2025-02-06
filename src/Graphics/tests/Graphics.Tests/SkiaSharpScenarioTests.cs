using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GraphicsTester.Scenarios;
using Microsoft.Maui.Graphics.Skia;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests;

public class SkiaSharpScenarioTests
{
	public static TheoryData<string> Scenarios =>
		new(ScenarioList.Scenarios.Select(s => s.ToString()));

	[Theory]
	[MemberData(nameof(Scenarios))]
	public void Scenario(string scenarioName)
	{
		// find scenario
		var scenario = ScenarioList.Scenarios.Single(s => s.ToString() == scenarioName);

		// render scenario
		using var bmp = new SkiaBitmapExportContext((int)scenario.Width, (int)scenario.Height, 1f);
		bmp.Canvas.FillColor = Colors.Transparent;
		bmp.Canvas.FillRectangle(0, 0, scenario.Width, scenario.Height);
		scenario.Draw(bmp.Canvas);

		var expectedImagePath = GetExpectedImageaPath(scenario);

		// save image if it did not exist, then fail the test
		if (!File.Exists(expectedImagePath))
		{
			var newImageFilename = Path.Combine(ProjectRoot, expectedImagePath);
			Directory.CreateDirectory(Path.GetDirectoryName(newImageFilename));

			bmp.WriteToFile(newImageFilename);

			Assert.Fail($"Image file did not exist, created: {newImageFilename}");
		}

		// file existed, compare
		ImageAssert.Equivalent(bmp.SKImage, expectedImagePath, GetErrorsImageDirectory(), 0);
	}

	private static string ProjectRoot =>
		Path.GetFullPath("../../../../../src/Graphics/tests/Graphics.Tests");

	private static string GetExpectedImageaPath(AbstractScenario scenario)
	{
		var fileName = GetSafeFilename(scenario.ToString()) + ".png";
		var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? "Windows"
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
				? "Mac"
				: "Linux";
		var filePath = Path.Combine("TestImages", os, fileName);

		return filePath;
	}

	private static string GetErrorsImageDirectory()
	{
		var outputFolder = Path.GetFullPath(Path.Combine(ProjectRoot, "Errors"));

		if (Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") is { } asd && !string.IsNullOrWhiteSpace(asd))
		{
			outputFolder = Path.GetFullPath(Path.Combine(
				asd, typeof(SkiaSharpScenarioTests).FullName, "Errors"));
		}

		Directory.CreateDirectory(outputFolder);

		return outputFolder;
	}

	private static string GetSafeFilename(string text)
	{
		char[] chars = text.ToCharArray();

		char[] allowedSpecialChars = ['_', '-'];
		for (int i = 0; i < chars.Length; i++)
		{
			chars[i] = char.IsLetterOrDigit(chars[i]) || allowedSpecialChars.Contains(chars[i])
				? char.ToLowerInvariant(chars[i])
				: '-';
		}

		var safe = new string(chars);
		while (safe.Contains("--", StringComparison.OrdinalIgnoreCase))
		{
			safe = safe.Replace("--", "-", StringComparison.OrdinalIgnoreCase);
		}

		return safe.Trim('-');
	}
}
