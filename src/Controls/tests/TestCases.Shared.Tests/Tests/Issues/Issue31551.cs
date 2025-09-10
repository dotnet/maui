using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31551 : _IssuesUITest
{
	public Issue31551(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ArgumentOutOfRangeException thrown by ScrollTo when group index is invalid";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyNoExceptionOnInvalidGroupIndex()
	{
		App.WaitForElement("Issue31551ScrollBtn");
		App.Tap("Issue31551ScrollBtn");
		
		var resultItem = App.WaitForElement("Issue31551StatusLabel").GetText();
		Assert.That(resultItem, Is.EqualTo("Success"));
	}
}