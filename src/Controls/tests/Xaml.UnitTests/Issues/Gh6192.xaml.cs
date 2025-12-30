using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6192 : ContentPage
{
	public Gh6192() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void XamlCDoesntFail(XamlInflator inflator)
		{
			var layout = new Gh6192(inflator);
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