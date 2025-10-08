using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31899 : _IssuesUITest
{
	public Issue31899(TestDevice device) : base(device) { }

	public override string Issue => "Header/Footer removed at runtime leaves empty space and EmptyView not resized in CollectionView";

#if !ANDROID		// More Info - https://github.com/dotnet/maui/issues/31911
	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void RemoveHeaderFooterAtRuntime()
	{
		App.WaitForElement("HeaderLabel");
		App.Click("ToggleHeaderButton");
		App.Click("ToggleFooterButton");
		VerifyScreenshot();
	}
#endif

#if IOS || MACCATALYST		// More Info - https://github.com/dotnet/maui/issues/31911
	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void AddHeaderFooterAtRuntime()
	{
		App.WaitForElement("HeaderLabel");
		App.Click("ToggleHeaderButton");
		App.Click("ToggleFooterButton");
		VerifyScreenshot();
	}
#endif
}