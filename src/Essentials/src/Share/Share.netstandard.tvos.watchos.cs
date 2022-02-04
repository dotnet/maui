using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Share']/Docs" />
	public static partial class Share
	{
		static Task PlatformRequestAsync(ShareTextRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformRequestAsync(ShareMultipleFilesRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
