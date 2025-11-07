using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FindByName : ContentPage
{
	public FindByName() => InitializeComponent();


	public class FindByNameTests
	{
		[Theory]
		[Values]
		public void TestRootName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
			Assert.Same(page, page.FindByName<FindByName>("root"));
		}

		[Theory]
		[Values]
		public void TestName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page.label0, page.FindByName<Label>("label0"));
		}
	}
}