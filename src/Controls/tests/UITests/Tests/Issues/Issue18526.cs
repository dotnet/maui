using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue18526 : _IssuesUITest
{
	public override string Issue => "Border not rendering inside a frame";

	public Issue18526(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void BorderShouldRender()
	{
		var label = App.WaitForElement("label");

		Assert.True(label.GetText() == ".NET MAUI");
	}
}
