using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13474
{
	public Maui13474() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void FontImageSourceIsAppliedFromSharedResources(XamlInflator inflator)
		{
			var page = new Maui13474(inflator);
			var fontImageSource = page.imageButton.Source as FontImageSource;
			Assert.Equal(fontImageSource.Color, Colors.Red);
			Assert.Equal("FontAwesome", fontImageSource.FontFamily);
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
