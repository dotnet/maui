using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11857 : ContentPage
{
	public Maui11857() => InitializeComponent();

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
		public void SolidColorBrushAsCompiledResources(XamlInflator inflator)
		{
			//shouldn't throw
			var page = new Maui11857(inflator);
			Assert.Equal(Colors.HotPink, ((SolidColorBrush)page.label.Background).Color);
		}
	}
}
