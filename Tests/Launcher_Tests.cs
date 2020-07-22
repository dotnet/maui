using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Launcher_Tests
    {
        [Fact]
        public async Task CanOpen_String_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.CanOpenAsync("http://www.xamarin.com"));

        [Fact]
        public async Task CanOpen_Uri_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.CanOpenAsync(new Uri("http://www.xamarin.com")));

        [Fact]
        public async Task Open_String_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.OpenAsync("http://www.xamarin.com"));

        [Fact]
        public async Task Open_Uri_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Launcher.OpenAsync(new Uri("http://www.xamarin.com")));

        [Fact]
        public async Task Open_File_NetStandard() =>
            await Assert.ThrowsAsync<ArgumentNullException>(() => Launcher.OpenAsync(new OpenFileRequest()));
    }
}
