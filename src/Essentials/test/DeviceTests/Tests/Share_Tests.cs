using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
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

#if ANDROID
		[Fact]
		public void Share_SingleFileIntent_HasClipData()
		{
			var file = Path.Combine(FileSystem.CacheDirectory, "share_clipdata_test.txt");
			File.WriteAllText(file, "ClipData test content");

			try
			{
				var request = new ShareFileRequest
				{
					Title = "Test Share",
					File = new ShareFile(file)
				};

				var intent = ShareImplementation.CreateShareFileIntent((ShareMultipleFilesRequest)request);

				if (OperatingSystem.IsAndroidVersionAtLeast(29))
				{
					Assert.NotNull(intent.ClipData);
					Assert.Equal(1, intent.ClipData.ItemCount);
					Assert.NotNull(intent.ClipData.GetItemAt(0)?.Uri);
				}
				else
				{
					Assert.Null(intent.ClipData);
				}
			}
			finally
			{
				File.Delete(file);
			}
		}

		[Fact]
		public void Share_MultipleFilesIntent_HasClipData()
		{
			var file1 = Path.Combine(FileSystem.CacheDirectory, "share_clipdata_test1.txt");
			var file2 = Path.Combine(FileSystem.CacheDirectory, "share_clipdata_test2.txt");
			File.WriteAllText(file1, "ClipData test content 1");
			File.WriteAllText(file2, "ClipData test content 2");

			try
			{
				var request = new ShareMultipleFilesRequest
				{
					Title = "Test Share Multiple",
					Files = new List<ShareFile>
					{
						new ShareFile(file1),
						new ShareFile(file2)
					}
				};

				var intent = ShareImplementation.CreateShareFileIntent(request);

				if (OperatingSystem.IsAndroidVersionAtLeast(29))
				{
					Assert.NotNull(intent.ClipData);
					Assert.Equal(2, intent.ClipData.ItemCount);
					Assert.NotNull(intent.ClipData.GetItemAt(0)?.Uri);
					Assert.NotNull(intent.ClipData.GetItemAt(1)?.Uri);
				}
				else
				{
					Assert.Null(intent.ClipData);
				}
			}
			finally
			{
				File.Delete(file1);
				File.Delete(file2);
			}
		}
#endif
	}
}
