#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Share']/Docs" />
	public static partial class Share
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][1]/Docs" />
		public static Task RequestAsync(string text) =>
			Current.RequestAsync(text);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][5]/Docs" />
		public static Task RequestAsync(string text, string title) =>
			Current.RequestAsync(text, title);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][4]/Docs" />
		public static Task RequestAsync(ShareTextRequest request) =>
			Current.RequestAsync(request);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][2]/Docs" />
		public static Task RequestAsync(ShareFileRequest request) =>
			Current.RequestAsync(request);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Share.xml" path="//Member[@MemberName='RequestAsync'][3]/Docs" />
		public static Task RequestAsync(ShareMultipleFilesRequest request) =>
			Current.RequestAsync(request);

		static IShare Current => ApplicationModel.DataTransfer.Share.Default;
	}
}
