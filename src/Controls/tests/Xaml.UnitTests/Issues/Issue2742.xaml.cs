using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Issue2742BasePage : ContentPage
{
}


public partial class Issue2742 : Issue2742BasePage
{
	public Issue2742() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ToolBarItemsOnContentPageInheritors(XamlInflator inflator)
		{
			var layout = new Issue2742(inflator);
			Assert.IsType<Label>(layout.Content);
			Assert.Equal("test", ((Label)layout.Content).Text);

			Assert.NotNull(layout.ToolbarItems);
			Assert.Equal(2, layout.ToolbarItems.Count);
			Assert.Equal("One", layout.ToolbarItems[0].Text);
		}
	}
}