using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh6192 : ContentPage
	{
		public Gh6192() => InitializeComponent();
		public Gh6192(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Fact]
			public void XamlCDoesntFail([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh6192(useCompiledXaml);
				layout.BindingContext = new
				{
					Items = new[] {
						new {
							Options = new [] { "Foo", "Bar" },
						}
					}
				};
				var lv = (layout.bindableStackLayout.Children[0] as ContentView).Content as ListView;
				lv.ItemTemplate.CreateContent();
			}
		}
	}
}