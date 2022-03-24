#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static partial class Browser
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(string uri) => Default.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(string uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(string uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(Uri uri) => Default.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		[Obsolete($"Use {nameof(Browser)}.{nameof(Default)} instead.", true)]
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);
	}
}
