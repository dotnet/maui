using System;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests
{
	public class BlazorWebViewAppTypeTests
	{
		sealed class DummyApp { }
		sealed class OtherApp { }

		[Fact]
		public void SettingAppTypeAppliesSyntheticHostPage()
		{
			var bwv = new BlazorWebView { AppType = typeof(DummyApp) };

			Assert.Equal("wwwroot/index.html", bwv.HostPage);
		}

		[Fact]
		public void ClearingAppTypeRemovesSyntheticHostPage()
		{
			var bwv = new BlazorWebView { AppType = typeof(DummyApp) };

			bwv.AppType = null;

			Assert.True(string.IsNullOrEmpty(bwv.HostPage));
		}

		[Fact]
		public void ExplicitHostPageIsNotOverwrittenByAppType()
		{
			var bwv = new BlazorWebView { HostPage = "custom.html", AppType = typeof(DummyApp) };

			Assert.Equal("custom.html", bwv.HostPage);
		}

		[Fact]
		public void ClearingAppTypeKeepsUserHostPageSetAfterAppType()
		{
			var bwv = new BlazorWebView { AppType = typeof(DummyApp) };
			// Caller overrides the synthetic host page after setting AppType.
			bwv.HostPage = "custom.html";

			bwv.AppType = null;

			Assert.Equal("custom.html", bwv.HostPage);
		}

		[Fact]
		public void ReassigningAppTypeDoesNotThrow()
		{
			var bwv = new BlazorWebView { AppType = typeof(DummyApp) };

			bwv.AppType = typeof(OtherApp);
			bwv.AppType = null;
			bwv.AppType = typeof(DummyApp);

			Assert.Equal(typeof(DummyApp), bwv.AppType);
			Assert.Equal("wwwroot/index.html", bwv.HostPage);
		}

		[Fact]
		public void SettingSameAppTypeTwiceIsNoOp()
		{
			var bwv = new BlazorWebView { AppType = typeof(DummyApp) };
			bwv.AppType = typeof(DummyApp);

			Assert.Equal(typeof(DummyApp), bwv.AppType);
			Assert.Equal("wwwroot/index.html", bwv.HostPage);
		}
	}
}
