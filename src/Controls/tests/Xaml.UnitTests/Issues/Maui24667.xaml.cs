using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24667 : ContentPage
{
	public Maui24667()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[XamlInflatorData]
		internal void ImplicitStyleBasedOnKeyedStyle(XamlInflator inflator)
		{
			var page = new Maui24667(inflator);
			Application.Current.MainPage = page;
			Assert.Equal(new Thickness(10, 7), page.button.Padding);
			Assert.Equal(new Thickness(10, 7), page.label.Padding);
		}
	}
}
