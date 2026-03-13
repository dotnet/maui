using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31818 : _IssuesUITest
{
	public Issue31818(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The MauiAsset does not work when both LogicalName and Link are specified";

	[Test]
	[Category(UITestCategories.ViewBaseTests)]
	public void VerifyMauiAssetWithLogicalNameAndLink()
	{
		App.WaitForElement("CheckFileButton");
		App.Tap("CheckFileButton");

		var statusText = App.WaitForElement("Issue31818_StatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Success"));
	}
}