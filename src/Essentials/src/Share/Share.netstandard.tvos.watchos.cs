using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		Task PlatformRequestAsync(ShareTextRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformRequestAsync(ShareFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformRequestAsync(ShareMultipleFilesRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
