using System;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
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
    }
}
