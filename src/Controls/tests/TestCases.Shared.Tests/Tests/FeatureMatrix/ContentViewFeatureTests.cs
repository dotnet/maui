using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class ContentViewFeatureTests : UITest
{
	public const string ContentViewFeatureMatrix = "ContentView Feature Matrix";

	public ContentViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ContentViewFeatureMatrix);
	}

	[Test]
	public void ContentViewWithFirstCustomPage()
	{
		App.WaitForElement("First Custom View");
	}

	[Test]
	public void ContentViewWithFirstCustomPageAndControlTemplate()
	{
		App.WaitForElement("FirstPageRadioButton");
		App.Tap("FirstPageRadioButton");
		App.WaitForElement("ControlTemplateYesRadioButton");
		App.Tap("ControlTemplateYesRadioButton");
		App.WaitForElement("First Control Template Applied");
	}

	[Test]
	public void ContentViewWithSecondCustomPage()
	{
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("Second Custom View");
	}

	[Test]
	public void ContentViewWithSecondCustomPageAndControlTemplate()
	{
		App.WaitForElement("SecondPageRadioButton");
		App.Tap("SecondPageRadioButton");
		App.WaitForElement("Second Custom View");
		App.WaitForElement("ControlTemplateYesRadioButton");
		App.Tap("ControlTemplateYesRadioButton");
		App.WaitForElement("Second Control Template Applied");
	}	
}