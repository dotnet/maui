using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Share_Tests
    {
        [Fact]
        public async Task Share_ShareTextRequestWithInvalidTextAndUri()
        {
            var request = new ShareTextRequest
            {
                Text = null,
                Uri = null
            };
            await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request));
        }

        [Fact]
        public async Task Share_NullShareTextRequest()
        {
            ShareTextRequest request = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync(request));
        }

        [Fact]
        public async Task Share_ShareFileRequestWithInvalidFile()
        {
            var request = new ShareFileRequest
            {
                File = null
            };
            await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request));
        }

        [Fact]
        public async Task Share_ShareFileRequestWithInvalidFilePath()
        {
            var request = new ShareFileRequest
            {
                File = new ShareFile(fullPath: null)
            };
            await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request));
        }

        [Fact]
        public async Task Share_NullShareFileRequest()
        {
            ShareFileRequest request = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync(request));
        }
    }
}
