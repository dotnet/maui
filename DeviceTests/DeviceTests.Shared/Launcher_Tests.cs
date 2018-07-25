using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    // TEST NOTES:
    //   - a human needs to close the browser window
    public class Launcher_Tests
    {
        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("http://example.com/?query=blah")]
        [InlineData("https://example.com/?query=blah")]
        [InlineData("mailto://someone@microsoft.com")]
        [InlineData("mailto://someone@microsoft.com?subject=test")]
        [InlineData("tel:+1 555 010 9999")]
        [InlineData("sms:5550109999")]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public Task Open(string uri)
        {
            return Utils.OnMainThread(() => Launcher.OpenAsync(uri));
        }

        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("http://example.com/?query=blah")]
        [InlineData("https://example.com/?query=blah")]
        [InlineData("mailto://someone@microsoft.com")]
        [InlineData("mailto://someone@microsoft.com?subject=test")]
        [InlineData("tel:+1 555 010 9999")]
        [InlineData("sms:5550109999")]
        public async Task CanOpen(string uri)
        {
#if __IOS__
            if (DeviceInfo.DeviceType == DeviceType.Virtual && (uri.Contains("tel:") || uri.Contains("mailto:")))
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
        [InlineData("mailto://someone@microsoft.com")]
        [InlineData("mailto://someone@microsoft.com?subject=test")]
        [InlineData("tel:+1 555 010 9999")]
        [InlineData("sms:5550109999")]
        public async Task CanOpenUri(string uri)
        {
#if __IOS__
            if (DeviceInfo.DeviceType == DeviceType.Virtual && (uri.Contains("tel:") || uri.Contains("mailto:")))
            {
                Assert.False(await Launcher.CanOpenAsync(new Uri(uri)));
                return;
            }

#endif

            Assert.True(await Launcher.CanOpenAsync(new Uri(uri)));
        }

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
    }
}
