// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6996 : ContentPage
{
	public Gh6996() => InitializeComponent();


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
		public void FontImageSourceColorWithDynamicResource(XamlInflator inflator)
		{
			var layout = new Gh6996(inflator);
			Image image = layout.image;
			var fis = image.Source as FontImageSource;
			Assert.Equal(Colors.Orange, fis.Color);

			layout.Resources["imcolor"] = layout.Resources["notBlue"];
			Assert.Equal(Colors.Lime, fis.Color);
		}
	}
}
