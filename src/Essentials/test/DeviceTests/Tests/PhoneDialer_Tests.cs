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
		[InlineData("5550109999", true)]
		[InlineData("+1 555 010 9999", true)]
		[InlineData("5550109999", false)]
		public void Open_Uses_Registered_Tel_Handler(string number, bool launchSucceeded)
		{
			var originalLauncher = Launcher.Default;
			var launcher = new RecordingLauncher(launchSucceeded);

			try
			{
				Launcher.SetDefault(launcher);

				PhoneDialer.Default.Open(number);

				Assert.NotNull(launcher.OpenedUri);
				Assert.Equal($"tel:{number}", launcher.OpenedUri.OriginalString);
			}
			finally
			{
				Launcher.SetDefault(originalLauncher);
			}
		}

		sealed class RecordingLauncher : ILauncher
		{
			readonly bool launchSucceeded;

			public RecordingLauncher(bool launchSucceeded) =>
				this.launchSucceeded = launchSucceeded;

			public Uri OpenedUri { get; private set; }

			public Task<bool> OpenAsync(Uri uri)
			{
				OpenedUri = uri;
				return Task.FromResult(launchSucceeded);
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
