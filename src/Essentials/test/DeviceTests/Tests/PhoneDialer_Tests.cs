using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("PhoneDialer")]
	[Collection(EssentialsStaticStateCollection.Name)]
	public class PhoneDialer_Tests
	{
#if WINDOWS
		[Theory]
		[InlineData("5550109999", "tel:5550109999")]
		[InlineData("+1 555 010 9999", "tel:+1 555 010 9999")]
		[InlineData("123#456", "tel:123%23456")]
		[InlineData("123?456", "tel:123%3F456")]
		[InlineData("123%23456", "tel:123%2523456")]
		[InlineData("+1-555-123-4567;ext=123", "tel:+1-555-123-4567;ext=123")]
		[InlineData("5550109999;phone-context=+1", "tel:5550109999;phone-context=+1")]
		[InlineData("+15550109999;ext=12#3", "tel:+15550109999;ext=12%233")]
		public void Open_Submits_Escaped_Number_To_Launcher(string number, string expectedUri)
		{
			var originalLauncher = Launcher.Default;
			var launcher = new RecordingLauncher();

			try
			{
				Launcher.SetDefault(launcher);

				PhoneDialer.Default.Open(number);

				Assert.NotNull(launcher.OpenedUri);
				Assert.Equal(expectedUri, launcher.OpenedUri.OriginalString);
				Assert.Equal(string.Empty, launcher.OpenedUri.Fragment);
				Assert.Equal(string.Empty, launcher.OpenedUri.Query);
				Assert.Equal(number, Uri.UnescapeDataString(launcher.OpenedUri.AbsolutePath));
			}
			finally
			{
				Launcher.SetDefault(originalLauncher);
			}
		}

		sealed class RecordingLauncher : ILauncher
		{
			public Uri OpenedUri { get; private set; }

			public Task<bool> OpenAsync(Uri uri)
			{
				OpenedUri = uri;
				return Task.FromResult(true);
			}

			public Task<bool> CanOpenAsync(Uri uri) =>
				throw new NotSupportedException();

			public Task<bool> OpenAsync(OpenFileRequest request) =>
				throw new NotSupportedException();

			public Task<bool> TryOpenAsync(Uri uri) =>
				throw new NotSupportedException();
		}
#endif
	}
}
