using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue28110 : _IssuesUITest
{
	public Issue28110(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Keyboard Does Not Close on Search Button Press in SearchBar Control";

    [Test]
	[Category(UITestCategories.SearchBar)]
	public void CarouselViewShouldNotCrash()
	{
		App.WaitForElement("MauiSearchBar");

        App.Tap("MauiSearchBar");

        App.DismissKeyboard();

        Assert.That(App.IsKeyboardShown(),Is.EqualTo(false));
	}
}
