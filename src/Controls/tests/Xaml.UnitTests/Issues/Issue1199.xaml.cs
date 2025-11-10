using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1199 : ContentPage
{
	public Issue1199() => InitializeComponent();


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
		public void AllowCreationOfTypesFromString(XamlInflator inflator)
		{
			var layout = new Issue1199(inflator);
			var res = (Color)layout.Resources["AlmostSilver"];

			Assert.Equal(Color.FromArgb("#FFCCCCCC"), res);
		}
	}
}