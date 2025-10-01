using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class BindableLayoutFeatureTests : UITest
{
	public const string BindableLayoutFeatureMatrix = "BindableLayout Feature Matrix";

	public BindableLayoutFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(BindableLayoutFeatureMatrix);
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void BindableLayoutFeatureMatrixRuns()
	{
		App.Screenshot("I am in BindableLayout Feature Matrix");
	}
}