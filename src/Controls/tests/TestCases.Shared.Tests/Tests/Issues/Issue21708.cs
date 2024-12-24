using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21708 : _IssuesUITest
{
	public Issue21708(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView.Scrolled event offset isn't correctly reset when items change on Android";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewVerticalOffset()
	{
		App.WaitForElement("Fill");
		App.ScrollDown("CollectionView");
		Assert.That(App.FindElement("Label").GetText(), Is.GreaterThan("0"));
		App.Tap("Empty");
		#if !MACCATALYST || !IOS   
		//When ItemSource is cleared, the VerticalOffset does not reset to zero on Mac and iOS.The offset updates normally after a new source is added
		//https://github.com/dotnet/maui/issues/26798
		Assert.That(App.FindElement("Label").GetText(), Is.EqualTo("0"));
		#endif
		App.Tap("Fill");
		App.ScrollDown("CollectionView");
		Assert.That(App.FindElement("Label").GetText(), Is.GreaterThan("0"));
	}
}