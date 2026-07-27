using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23902 : _IssuesUITest
{
	public Issue23902(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "NavigationPage and FlyoutPage both call OnNavigatedTo, so it is called twice";

	[Test]
	[Category(UITestCategories.Navigation)]
	public async Task Issue23902NavigationTest()
	{
		App.WaitForElement("Go to Third Page");
		App.Tap("ThirdPageButton");
		Assert.That(App.WaitForElement("HeaderLabel").GetText(), Is.EqualTo("NavigatedTo called 1 times"));
		App.WaitForElement("SecondPageButton");
		App.Tap("SecondPageButton");
		App.WaitForElement("Go to Third Page");
		App.Tap("ThirdPageButton");
		Assert.That(App.WaitForElement("HeaderLabel").GetText(), Is.EqualTo("NavigatedTo called 2 times"));
		App.WaitForElement("Go to 4th Page");
		App.Tap("FourthPageButton");
		App.WaitForElement("Go Back to 3rd Page");
		App.Tap("ThirdPageButton");
		Assert.That(App.WaitForElement("HeaderLabel").GetText(), Is.EqualTo("NavigatedTo called 3 times"));
	}
}
