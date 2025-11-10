using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11204 : ContentPage
{
	public Maui11204() => InitializeComponent();

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
		public void VSMSetterOverrideManualValues(XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.Equal(Colors.FloralWhite, page.border.BackgroundColor);
			VisualStateManager.GoToState(page.border, "State1");
			Assert.Equal(2, page.border.StrokeThickness);
			Assert.Equal(Colors.Blue, page.border.BackgroundColor);
		}

		[Theory]
		[Values]
		public void StyleVSMSetterOverrideManualValues(XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.Equal(Colors.HotPink, page.borderWithStyleClass.BackgroundColor);
			VisualStateManager.GoToState(page.borderWithStyleClass, "State1");
			Assert.Equal(2, page.borderWithStyleClass.StrokeThickness);
			Assert.Equal(Colors.Blue, page.borderWithStyleClass.BackgroundColor);
		}
	}
}