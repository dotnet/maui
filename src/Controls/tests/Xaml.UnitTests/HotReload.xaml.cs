using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReload : ContentPage
{
	public HotReload() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
		}

#if DEBUG
		[Test]
		public void HotReloadWorks([Values(XamlInflator.Runtime, XamlInflator.SourceGen)] XamlInflator inflator)
		{
			var page = new HotReload(inflator);
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Lime));

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

			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.HotPink));

		}
#endif
	}
}