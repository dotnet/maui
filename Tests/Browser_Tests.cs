using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class BrowserTests
    {
        [Fact]
        public async Task Open_Uri_String_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Browser.OpenAsync("http://xamarin.com"));

        [Fact]
        public async Task Open_Uri_String_Launch_NetStandard() =>
             await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Browser.OpenAsync("http://xamarin.com", BrowserLaunchType.SystemPreferred));

        [Fact]
        public async Task Open_Uri_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Browser.OpenAsync(new Uri("http://xamarin.com")));

        [Fact]
        public async Task Open_Uri_Launch_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Browser.OpenAsync(new Uri("http://xamarin.com"), BrowserLaunchType.SystemPreferred));

        [Theory]
        [InlineData("https://xamarin.com", "https://xamarin.com")]
        [InlineData("http://xamarin.com", "http://xamarin.com")]
        [InlineData("https://xamariñ.com", "https://xn--xamari-1wa.com")]
        [InlineData("http://xamariñ.com", "http://xn--xamari-1wa.com")]
        [InlineData("https://xamariñ.com/?test=xamariñ", "https://xn--xamari-1wa.com/?test=xamari%C3%B1")]
        [InlineData("http://xamariñ.com/?test=xamariñ", "http://xn--xamari-1wa.com/?test=xamari%C3%B1")]
        [InlineData("http://xamariñ.com/?test=xamariñ xamariñ", "http://xn--xamari-1wa.com/?test=xamari%C3%B1%20xamari%C3%B1")]
        public void Escape_Uri(string uri, string escaped)
        {
            var escapedUri = Browser.EscapeUri(new Uri(uri));

            Assert.Equal(escaped, escapedUri.AbsoluteUri.TrimEnd('/'));
        }
    }
}
