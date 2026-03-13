#if IOS || ANDROID //SetOrientation is only supported on iOS and Android; orientation change is not available on Windows and MacCatalyst.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20042 : _IssuesUITest
{
	public Issue20042(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] ScrollView HorizontalOptions=StartAndExpand does not expand to fill screen width in landscape";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewShouldFillScreenWidthInLandscape()
	{
		App.WaitForElement("InnerGrid");

		var portraitGridRect = App.WaitForElement("InnerGrid").GetRect();
		var portraitRefRect = App.WaitForElement("ReferenceBar").GetRect();

		Assert.That(portraitGridRect.Width, Is.EqualTo(portraitRefRect.Width).Within(5),
			$"Portrait: InnerGrid width ({portraitGridRect.Width}) should match full page width ({portraitRefRect.Width})");

		try
		{
			App.SetOrientationLandscape();
			App.WaitForElement("InnerGrid");

			var landscapeGridRect = App.WaitForElement("InnerGrid").GetRect();
			var landscapeRefRect = App.WaitForElement("ReferenceBar").GetRect();

			Assert.That(landscapeGridRect.Width, Is.EqualTo(landscapeRefRect.Width).Within(5),
				$"Landscape: InnerGrid width ({landscapeGridRect.Width}) should match full screen width ({landscapeRefRect.Width})");

			Assert.That(landscapeGridRect.Width, Is.GreaterThan(portraitGridRect.Width),
				"Landscape width should be greater than portrait width");
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}
}

#endif
