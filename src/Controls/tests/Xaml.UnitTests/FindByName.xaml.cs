using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class FindByName : ContentPage
{
	public FindByName() => InitializeComponent();

	[TestFixture]
	public class FindByNameTests
	{
		[Test]
		public void TestRootName([Values]XamlInflator useCompiledXaml)
		{
			var page = new FindByName(useCompiledXaml);
			Assert.AreSame(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
			Assert.AreSame(page, page.FindByName<FindByName>("root"));
		}

		[Test]
		public void TestName([Values]XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.AreSame(page.label0, page.FindByName<Label>("label0"));
		}
	}
}