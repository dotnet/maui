using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class U001Page : ContentPage
{
	public U001Page()
	{
	}

}

public partial class Unreported001 : TabbedPage
{
	public Unreported001() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void DoesNotThrow([Values] XamlInflator inflator)
		{
			var p = new Unreported001(inflator);
			Assert.That(p.navpage.CurrentPage, Is.TypeOf<U001Page>());
		}
	}
}