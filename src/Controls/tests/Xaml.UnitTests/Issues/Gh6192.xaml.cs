using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6192 : ContentPage
{
	public Gh6192() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}


		[Theory]
		[Values]
		public void XamlCDoesntFail(XamlInflator inflator)
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