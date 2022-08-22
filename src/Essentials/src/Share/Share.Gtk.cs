using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Share
	{
		static Task PlatformRequestAsync(ShareTextRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformRequestAsync(ShareMultipleFilesRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
