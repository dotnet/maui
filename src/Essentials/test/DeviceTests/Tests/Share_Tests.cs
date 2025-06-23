using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Share")]
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
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_NullShareTextRequest()
		{
			ShareTextRequest request = null;
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_ShareFileRequestWithInvalidFile()
		{
			var request = new ShareFileRequest
			{
				File = null
			};
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_NullShareFileRequest()
		{
			ShareFileRequest request = null;
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_ShareMultipleFilesRequestWithEmptyFilesList()
		{
			var request = new ShareMultipleFilesRequest
			{
				Files = new List<ShareFile>()
			};
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_ShareMultipleFilesRequestWithInvalidFilesList()
		{
			var request = new ShareMultipleFilesRequest
			{
				Files = new List<ShareFile>() { null }
			};
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public async Task Share_NullShareMultipleFilesRequest()
		{
			ShareMultipleFilesRequest request = null;
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync(request)).ConfigureAwait(false);
		}

		[Fact]
		public void Share_FiletWithInvalidFilePath()
			=> Assert.Throws<ArgumentException>(() => new ShareFile(fullPath: " "));

		[Fact]
		public void Share_FiletWithNullFilePath()
			=> Assert.Throws<ArgumentNullException>(() => new ShareFile(fullPath: null));
	}
}
