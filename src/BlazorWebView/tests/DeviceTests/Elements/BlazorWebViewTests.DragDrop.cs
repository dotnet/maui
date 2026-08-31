using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if ANDROID
	[Fact]
	public void ImageDragDropContentProviderIsAvailable()
	{
		var context = global::Android.App.Application.Context;
		var authority = $"{context.PackageName}.DropDataProvider";
		var provider = context.PackageManager?.ResolveContentProvider(
			authority,
			global::Android.Content.PM.PackageInfoFlags.MetaData);

		Assert.NotNull(provider);
		Assert.Equal("androidx.webkit.DropDataContentProvider", provider.Name);
		Assert.False(provider.Exported);
		Assert.True(provider.GrantUriPermissions);

		var contentResolver = context.ContentResolver;
		Assert.NotNull(contentResolver);

		using var providerClient = contentResolver.AcquireUnstableContentProviderClient(authority);
		Assert.NotNull(providerClient);

		using var probeUri = global::Android.Net.Uri.Parse($"content://{authority}/maui-probe");
		Assert.NotNull(probeUri);

		// Force the provider to initialize its bridge to the WebView implementation.
		_ = contentResolver.GetType(probeUri);
	}
#endif
}
