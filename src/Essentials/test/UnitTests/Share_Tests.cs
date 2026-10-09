using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
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

		[Theory]
		[InlineData(null, null)]
		[InlineData(null, "")]
		[InlineData("", null)]
		[InlineData("", "")]
		public async Task Request_Text_WithInvalidTextAndUri(string text, string uri) =>
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(new ShareTextRequest
			{
				Text = text,
				Uri = uri
			}));

		[Fact]
		public async Task Request_File_Request_NetStandard() =>
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(new ShareFileRequest()));

		[Fact]
		public async Task Request_Multiple_Files_Request_NetStandard() =>
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(new ShareMultipleFilesRequest()));

		[Fact]
		public async Task Share_NullShareTextRequest() =>
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync((ShareTextRequest)null));

		[Fact]
		public async Task Share_NullShareFileRequest() =>
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync((ShareFileRequest)null));

		[Fact]
		public async Task Share_NullShareMultipleFilesRequest() =>
			await Assert.ThrowsAsync<ArgumentNullException>(() => Share.RequestAsync((ShareMultipleFilesRequest)null));

		[Fact]
		public async Task Share_ShareMultipleFilesRequestWithInvalidFilesList() =>
			await Assert.ThrowsAsync<ArgumentException>(() => Share.RequestAsync(new ShareMultipleFilesRequest
			{
				Files = new List<ShareFile> { null }
			}));

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		public void Share_FileWithInvalidFilePath(string path) =>
			Assert.Throws<ArgumentException>(() => new ShareFile(path));

		[Fact]
		public void Share_FileWithNullFilePath() =>
			Assert.Throws<ArgumentNullException>(() => new ShareFile(fullPath: null));
	}
}
