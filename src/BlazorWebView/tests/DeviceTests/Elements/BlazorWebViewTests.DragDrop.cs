using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if ANDROID
	[Fact]
	public void ImageDragDropContentProviderIsRegistered()
	{
		var context = global::Android.App.Application.Context;
		var provider = context.PackageManager?.ResolveContentProvider(
			$"{context.PackageName}.DropDataProvider",
			global::Android.Content.PM.PackageInfoFlags.MetaData);

		Assert.NotNull(provider);
		Assert.Equal("androidx.webkit.DropDataContentProvider", provider.Name);
		Assert.False(provider.Exported);
		Assert.True(provider.GrantUriPermissions);
	}
#endif
}
