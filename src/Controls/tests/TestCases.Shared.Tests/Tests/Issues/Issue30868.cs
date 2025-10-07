#if TEST_FAILS_ON_WINDOWS // Using AppThemeBinding and changing theme not working on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30868 : _IssuesUITest
{
	public Issue30868(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView selection visual states";

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionModeOnDarkTheme()
	{
		try
		{
			App.SetDarkTheme();
			App.WaitForElement("Item 2");
			App.Tap("Item 2");
			VerifyScreenshot();
		}
		finally
		{
			App.SetLightTheme();
		}
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionModeOnLightTheme()
	{
		App.WaitForElement("Item 2");
		VerifyScreenshot();
	}
}
#endif