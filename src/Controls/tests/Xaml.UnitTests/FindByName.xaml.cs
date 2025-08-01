using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FindByName : ContentPage
{
	public FindByName() => InitializeComponent();

	[TestFixture]
	class FindByNameTests
	{
		[Test]
		public void TestRootName([Values] XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.AreSame(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
			Assert.AreSame(page, page.FindByName<FindByName>("root"));
		}

		[Test]
		public void TestName([Values] XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.AreSame(page.label0, page.FindByName<Label>("label0"));
		}
	}
}