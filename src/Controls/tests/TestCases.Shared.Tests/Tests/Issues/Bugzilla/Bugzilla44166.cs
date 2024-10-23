using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla44166 : _IssuesUITest
{

	public Bugzilla44166(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage instances do not get disposed upon GC";

	// TODO From Xamarin.UITest Migration: this test references elements directly, needs to be rewritten
	// [Test]
	// [Category(UITestCategories.Performance)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void Bugzilla44166Test()
	// {
	// 	App.WaitForElement("Go");
	// 	App.Tap("Go");

	// 	App.WaitForElement("Previous");
	// 	App.Tap("Previous");

	// 	App.WaitForElement("GC");

	// 	for (var n = 0; n < 10; n++)
	// 	{
	// 		App.Tap("GC");

	// 		if (App.FindElements(("Success")).Count > 0)
	// 		{
	// 			return;
	// 		}
	// 	}

	// 	string pageStats = string.Empty;

	// 	if (_44166MDP.Counter > 0)
	// 	{
	// 		pageStats += $"{_44166MDP.Counter} {nameof(_44166MDP)} allocated; ";
	// 	}

	// 	if (_44166Master.Counter > 0)
	// 	{
	// 		pageStats += $"{_44166Master.Counter} {nameof(_44166Master)} allocated; ";
	// 	}

	// 	if (_44166Detail.Counter > 0)
	// 	{
	// 		pageStats += $"{_44166Detail.Counter} {nameof(_44166Detail)} allocated; ";
	// 	}

	// 	if (_44166NavContent.Counter > 0)
	// 	{
	// 		pageStats += $"{_44166NavContent.Counter} {nameof(_44166NavContent)} allocated; ";
	// 	}

	// 	Assert.Fail($"At least one of the pages was not collected: {pageStats}");
	// }
}