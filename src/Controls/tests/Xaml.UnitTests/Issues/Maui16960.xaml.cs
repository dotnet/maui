using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16960
{
	public Maui16960() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void VSMandAppTheme(XamlInflator inflator)
		{

			Application.Current.UserAppTheme = AppTheme.Light;

			var page = new Maui16960(inflator);
			Button button = page.button;
			Assert.Null(button.BackgroundColor);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.Equal(Colors.Red, button.BackgroundColor);

			VisualStateManager.GoToState(button, "Pressed");
			Assert.Equal(Colors.Yellow, button.BackgroundColor);

			VisualStateManager.GoToState(button, "Normal");
			Assert.Null(button.BackgroundColor);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.Equal(Colors.Red, button.BackgroundColor);


		}
	}
}