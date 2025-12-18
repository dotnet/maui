using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReload : ContentPage
{
	public HotReload() => InitializeComponent();
	
	[Collection("Xaml Inflation")]
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
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
		}

#if DEBUG
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void HotReloadWorks(XamlInflator inflator)
		{
			var page = new HotReload(inflator);
			Assert.Equal(Colors.Lime, page.label0.BackgroundColor);

			var updatedXaml =
"""
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.HotReload"
             Title="HotReload">
    <VerticalStackLayout>
        <Label
            x:Name="label0"
            Text="Welcome to .NET MAUI!"
            VerticalOptions="Center" 
            BackgroundColor="HotPink"
            HorizontalOptions="Center" />
    </VerticalStackLayout>
</ContentPage>
""";

			Controls.Internals.ResourceLoader.ResourceProvider2 = (query) =>
			{
				if (query.ResourcePath.EndsWith("HotReload.xaml"))
				{
					return new Controls.Internals.ResourceLoader.ResourceLoadingResponse
					{
						ResourceContent = updatedXaml,
						UseDesignProperties = false
					};
				}
				return null;
			};

			page = new HotReload(inflator);

			Assert.Equal(Colors.HotPink, page.label0.BackgroundColor);

		}
#endif
	}
}