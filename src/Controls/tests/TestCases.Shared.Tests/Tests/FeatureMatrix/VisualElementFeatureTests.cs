using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Visual)]
public class VisualElementFeatureTests : UITest
{
	public const string VisualElementFeatureMatrix = "VisualElement Feature Matrix";

	public VisualElementFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(VisualElementFeatureMatrix);
	}
}