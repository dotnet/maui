using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13474
{
	public Maui13474() => InitializeComponent();

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
		internal void FontImageSourceIsAppliedFromSharedResources(XamlInflator inflator)
		{
			var page = new Maui13474(inflator);
			var fontImageSource = page.imageButton.Source as FontImageSource;
			Assert.Equal(Colors.Red, fontImageSource.Color);
			Assert.Equal("FontAwesome", fontImageSource.FontFamily);
		}
	}
}
