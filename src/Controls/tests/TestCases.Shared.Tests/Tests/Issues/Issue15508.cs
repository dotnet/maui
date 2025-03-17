using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15508 : _IssuesUITest
{
	public Issue15508(TestDevice device) : base(device) { }

	public override string Issue => "Scrollview.ScrollTo execution only returns after manual scroll";
	const string ButtonToScroll = "ButtonToScroll";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewinAwait()
	{
		App.WaitForElement(ButtonToScroll);
		App.Tap(ButtonToScroll);
		Assert.That(App.FindElement("ScrollLabel").GetText(), Is.EqualTo("The text is successfully changed"));
	}
}

