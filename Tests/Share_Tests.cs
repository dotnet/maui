using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Share_Tests
    {
        [Fact]
        public async Task Request_Text_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Share.RequestAsync("Text"));

        [Fact]
        public async Task Request_Text_Title_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Share.RequestAsync("Text", "Title"));

        [Fact]
        public async Task Request_Text_Request_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Share.RequestAsync(new ShareTextRequest()));

        [Fact]
        public async Task Request_File_Request_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Share.RequestAsync(new ShareFileRequest()));
    }
}
