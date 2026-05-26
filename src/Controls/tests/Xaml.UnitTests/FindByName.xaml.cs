using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FindByName : ContentPage
{
	public FindByName() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class FindByNameTests
	{
		[Theory]
		[XamlInflatorData]
		internal void TestRootName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
			Assert.Same(page, page.FindByName<FindByName>("root"));
		}

		[Theory]
		[XamlInflatorData]
		internal void TestName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page.label0, page.FindByName<Label>("label0"));
		}
	}
}