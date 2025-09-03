using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Issue2742BasePage : ContentPage
{
}


public partial class Issue2742 : Issue2742BasePage
{
	public Issue2742() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ToolBarItemsOnContentPageInheritors([Values] XamlInflator inflator)
		{
			var layout = new Issue2742(inflator);
			Assert.That(layout.Content, Is.TypeOf<Label>());
			Assert.AreEqual("test", ((Label)layout.Content).Text);

			Assert.NotNull(layout.ToolbarItems);
			Assert.AreEqual(2, layout.ToolbarItems.Count);
			Assert.AreEqual("One", layout.ToolbarItems[0].Text);
		}
	}
}