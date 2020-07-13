using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	[Category(UITestCategories.ExpanderView)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
	internal class ExpanderViewUITests : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery("* marked:'Expander Gallery'");
		}

		[TestCase]
		public void ExpanderView()
		{
			App.Tap("EnableExpander");
			App.Tap("Expander Gallery");
			App.WaitForElement("The Second", "");
			App.Tap("Expander Level 2");
			App.WaitForElement("Hi, I am Red", "View didn't expand the second level");
			App.ScrollDownTo("The Fourth");
			App.Tap("The Fourth");
			App.WaitForNoElement("Hi, I am Red", "View didn't collapse like is should");
			App.WaitForElement("Expander Level 2", "Fourth view didn't expand to show 'Expander level 2'");
			App.Tap("Expander Level 2");
			App.WaitForElement("Hi, I am Red", "Expander level 2 of Fourth view didn't expand like it should.");
			App.Tap("Expander Level 2");
			App.WaitForNoElement("Hi, I am Red", "View didn't collapse like is should");
			App.Tap("The Fourth");
			App.WaitForNoElement("Expander Level 2", "View didn't collapse like is should");
			App.Back();
		}
	}
}