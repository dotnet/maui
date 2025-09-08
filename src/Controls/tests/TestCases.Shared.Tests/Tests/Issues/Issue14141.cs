using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14141 : _IssuesUITest
{
	public Issue14141(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ArgumentOutOfRangeException thrown by ScrollTo when group index is invalid";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyNoExceptionOnInvalidGroupIndex()
	{
		App.WaitForElement("Issue14141ScrollBtn");
		App.Tap("Issue14141ScrollBtn");
		
		var resultItem = App.WaitForElement("Issue14141StatusLabel").GetText();
		Assert.That(resultItem, Is.EqualTo("Success"));
	}
}