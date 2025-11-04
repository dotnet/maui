using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1213 : TabbedPage
{
	public Issue1213() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void MultiPageAsContentPropertyAttribute([Values] XamlInflator inflator)
		{
			var page = new Issue1213(inflator);
			Assert.AreEqual(2, page.Children.Count);
		}
	}
}