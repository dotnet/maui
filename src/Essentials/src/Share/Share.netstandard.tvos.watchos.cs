using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Share']/Docs" />
	public class ShareImplementation : IShare
	{
		public Task RequestAsync(ShareTextRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task RequestAsync(ShareMultipleFilesRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
