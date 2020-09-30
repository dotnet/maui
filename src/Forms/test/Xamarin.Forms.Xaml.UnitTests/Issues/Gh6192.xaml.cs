using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh6192 : ContentPage
	{
		public Gh6192() => InitializeComponent();
		public Gh6192(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
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