using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui40328 : ContentPage
{
	public Maui40328() => InitializeComponent();

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

		[Fact]
		public void InlineEventHandlerRuns()
		{
			var page = new Maui40328();
			Assert.Equal("Ready", page.status.Text);

			((IButtonController)page.button).SendClicked();

			Assert.Equal(1, page.ClickCount);
			Assert.Equal("Clicked 1", page.status.Text);
		}

		[Fact]
		public void RuntimeTypeLoadDoesNotIgnoreUncompiledCode()
		{
			const string xaml = """
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
					<x:Code><![CDATA[void Handler() { }]]></x:Code>
				</ContentPage>
				""";
			Controls.Internals.ResourceLoader.ResourceProvider2 = _ =>
				new Controls.Internals.ResourceLoader.ResourceLoadingResponse { ResourceContent = xaml };

			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(typeof(ContentPage)));
		}

#if DEBUG
		[Fact]
		public void InlineCodeDoesNotBreakHotReloadFallback()
		{
			const string updatedXaml = """
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
				             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui40328">
					<x:Code><![CDATA[
						void OnClicked(object sender, System.EventArgs e) { }
					]]></x:Code>
					<VerticalStackLayout>
						<Button x:Name="button" Text="Updated" Clicked="OnClicked" />
						<Label x:Name="status" Text="Reloaded" />
					</VerticalStackLayout>
				</ContentPage>
				""";
			Controls.Internals.ResourceLoader.ResourceProvider2 = query =>
				query.ResourcePath.EndsWith("Maui40328.sgen.xaml", StringComparison.Ordinal)
					? new Controls.Internals.ResourceLoader.ResourceLoadingResponse { ResourceContent = updatedXaml }
					: null;

			var page = new Maui40328();
			Assert.Equal("Reloaded", page.status.Text);

			((IButtonController)page.button).SendClicked();

			Assert.Equal(1, page.ClickCount);
			Assert.Equal("Clicked 1", page.status.Text);
		}
#endif
	}
}
