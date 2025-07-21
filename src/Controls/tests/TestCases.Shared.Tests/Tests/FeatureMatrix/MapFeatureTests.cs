using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class MapFeatureTests : UITest
{
	public const string MapFeatureMatrix = "Map Feature Matrix";

	public MapFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(MapFeatureMatrix);
	}
}