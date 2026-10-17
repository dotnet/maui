#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ImageButtonAspectVerificationTests : _GalleryUITest
{
	public ImageButtonAspectVerificationTests(TestDevice device) : base(device)
	{
	}

	public override string GalleryPageName => ImageButtonFeatureTests.ImageButtonFeatureMatrix;

	[TestCase("File", Aspect.AspectFit)]
	[TestCase("File", Aspect.AspectFill)]
	[TestCase("File", Aspect.Fill)]
	[TestCase("File", Aspect.Center)]
	[TestCase("Uri", Aspect.AspectFit)]
	[TestCase("Uri", Aspect.AspectFill)]
	[TestCase("Uri", Aspect.Fill)]
	[TestCase("Uri", Aspect.Center)]
	[TestCase("Stream", Aspect.AspectFit)]
	[TestCase("Stream", Aspect.AspectFill)]
	[TestCase("Stream", Aspect.Fill)]
	[TestCase("Stream", Aspect.Center)]
	[TestCase("FontImage", Aspect.AspectFit)]
	[TestCase("FontImage", Aspect.AspectFill)]
	[TestCase("FontImage", Aspect.Fill)]
	[TestCase("FontImage", Aspect.Center)]
	[Category(UITestCategories.ImageButton)]
	public void CaptureWindowsImageButtonAspects(string source, Aspect aspect)
	{
		App.WaitForElement(ImageButtonFeatureTests.Options);
		App.Tap(ImageButtonFeatureTests.Options);
		var aspectOption = aspect switch
		{
			Aspect.AspectFit => ImageButtonFeatureTests.ImageAspectFit,
			Aspect.AspectFill => ImageButtonFeatureTests.ImageAspectFill,
			Aspect.Fill => ImageButtonFeatureTests.ImageFill,
			Aspect.Center => ImageButtonFeatureTests.ImageCenter,
			_ => throw new ArgumentOutOfRangeException(nameof(aspect))
		};
		App.WaitForElement(aspectOption);
		App.Tap(aspectOption);
		App.Tap("SourceType" + source);
		App.Tap(ImageButtonFeatureTests.Apply);
		App.WaitForElement("ImageButtonControl");
		App.Tap("CaptureAspectPixels");
		App.WaitForElement("AspectCaptureReady", timeout: TimeSpan.FromSeconds(45));

		var directory = App.FindElement("AspectEvidenceDirectory").GetText()
			?? throw new InvalidOperationException("The application did not expose its evidence directory.");
		var artifactRoot = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")
			?? TestContext.CurrentContext.WorkDirectory;
		var output = Path.Combine(artifactRoot, "Issue29959", source + "_" + aspect);
		Directory.CreateDirectory(output);
		var screen = App.Screenshot(source + "_" + aspect);
		var screenPath = Path.Combine(output, "screen.png");
		File.Copy(screen.FullName, screenPath, true);
		TestContext.AddTestAttachment(screenPath);
		Assert.That(Directory.Exists(directory), Is.True, "The application evidence directory must exist.");
		foreach (var file in Directory.GetFiles(directory))
		{
			if (!Path.GetFileName(file).StartsWith(source + "_" + aspect, StringComparison.Ordinal) &&
				Path.GetFileName(file) != "capture-error.txt")
				continue;
			var destination = Path.Combine(output, Path.GetFileName(file));
			File.Copy(file, destination, true);
			TestContext.AddTestAttachment(destination);
		}
		Assert.That(App.FindElement("AspectCaptureReady").GetText(), Is.EqualTo($"CAPTURED {source}_{aspect}"));
		Assert.That(File.Exists(Path.Combine(output, source + "_" + aspect + ".json")), Is.True);
	}
}
#endif
