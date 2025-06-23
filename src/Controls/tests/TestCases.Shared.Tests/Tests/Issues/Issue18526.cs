using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18526 : _IssuesUITest
{
	public override string Issue => "Border not rendering inside a frame";

	public Issue18526(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Frame)]
	public void BorderShouldRender()
	{
		var label = App.WaitForElement("label");
		var size = label.GetRect();
		Assert.That(label.GetText(), Is.EqualTo(".NET MAUI"));
		Assert.That(size.Width, Is.GreaterThan(0));
		Assert.That(size.Height, Is.GreaterThan(0));
	}
}