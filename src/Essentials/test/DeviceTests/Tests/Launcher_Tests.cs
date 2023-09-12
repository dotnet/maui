using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// TEST NOTES:
	//   - a human needs to close the browser window
	[Category("Launcher")]
	public class Launcher_Tests
	{
		[Theory]
		[InlineData("http://www.example.com")]
		[InlineData("http://example.com/?query=blah")]
		[InlineData("https://example.com/?query=blah")]
		[InlineData("mailto://someone@microsoft.com"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("mailto://someone@microsoft.com?subject=test"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("tel:+1 555 010 9999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("sms:5550109999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Open(string uri)
		{
			return Utils.OnMainThread(() => Launcher.OpenAsync(uri));
		}

		[Theory]
		[InlineData("http://www.example.com")]
		[InlineData("http://example.com/?query=blah")]
		[InlineData("https://example.com/?query=blah")]
		[InlineData("mailto://someone@microsoft.com"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("mailto://someone@microsoft.com?subject=test"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("tel:+1 555 010 9999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("sms:5550109999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		public async Task CanOpen(string uri)
		{
#if __IOS__
			if (DeviceInfo.DeviceType == DeviceType.Virtual && (uri.Contains("tel:", StringComparison.Ordinal) || uri.Contains("mailto:", StringComparison.Ordinal)))
			{
				Assert.False(await Launcher.CanOpenAsync(uri));
				return;
			}
#endif

			Assert.True(await Launcher.CanOpenAsync(uri));
		}

		[Theory]
		[InlineData("http://www.example.com")]
		[InlineData("http://example.com/?query=blah")]
		[InlineData("https://example.com/?query=blah")]
		[InlineData("mailto://someone@microsoft.com"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("mailto://someone@microsoft.com?subject=test"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("tel:+1 555 010 9999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		[InlineData("sms:5550109999"
#if WINDOWS
			, Skip = "Doesn't work on Windows on CI"
#endif
			)]
		public async Task CanOpenUri(string uri)
		{
#if __IOS__
			if (DeviceInfo.DeviceType == DeviceType.Virtual && (uri.Contains("tel:", StringComparison.Ordinal) || uri.Contains("mailto:", StringComparison.Ordinal)))
			{
				Assert.False(await Launcher.CanOpenAsync(new Uri(uri)));
				return;
			}

#endif

			Assert.True(await Launcher.CanOpenAsync(new Uri(uri)));
		}

#if __IOS__
		[Theory]
		[InlineData("https://maps.apple.com/maps?q=Ole Vigs Gate 8B", "https://maps.apple.com/maps?q=Ole%20Vigs%20Gate%208B")]
		[InlineData("https://maps.apple.com", "https://maps.apple.com")]
		public void GetNativeUrl(string uri, string expected)
		{
			var url = WebUtils.GetNativeUrl(new Uri(uri));
			Assert.Equal(expected, url.AbsoluteString);
		}
#endif

		[Theory]
		[InlineData("Not Valid Uri")]
		public async Task InvalidUri(string uri)
		{
			await Assert.ThrowsAsync<UriFormatException>(() => Launcher.CanOpenAsync(uri));
		}

		[Theory]
		[InlineData("ms-invalidurifortest:abc")]
		public async Task CanNotOpenUri(string uri)
		{
			Assert.False(await Launcher.CanOpenAsync(new Uri(uri)));
		}

		[Theory]
		[InlineData("ms-invalidurifortest:abc")]
		public async Task CanNotOpen(string uri)
		{
			Assert.False(await Launcher.CanOpenAsync(uri));
		}

		[Theory]
		[InlineData("http://www.example.com")]
		[InlineData("http://example.com/?query=blah")]
		[InlineData("https://example.com/?query=blah")]
		[InlineData("mailto://someone@microsoft.com")]
		[InlineData("mailto://someone@microsoft.com?subject=test")]
		[InlineData("tel:+1 555 010 9999")]
		[InlineData("sms:5550109999")]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task TryOpen(string uri)
		{
#if __IOS__
			if (DeviceInfo.DeviceType == DeviceType.Virtual && (uri.Contains("tel:", StringComparison.Ordinal) || uri.Contains("mailto:", StringComparison.Ordinal)))
			{
				Assert.False(await Launcher.TryOpenAsync(uri));
				return;
			}
#endif

			Assert.True(await Launcher.TryOpenAsync(uri));
		}

		[Theory]
		[InlineData("ms-invalidurifortest:abc")]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task CanNotTryOpen(string uri)
		{
			Assert.False(await Launcher.TryOpenAsync(new Uri(uri)));
		}
	}
}
