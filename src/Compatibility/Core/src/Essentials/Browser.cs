#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static partial class Browser
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task<bool> OpenAsync(string uri) => Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task<bool> OpenAsync(string uri, BrowserLaunchMode launchMode) => Current.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task<bool> OpenAsync(string uri, BrowserLaunchOptions options) => Current.OpenAsync(uri, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task<bool> OpenAsync(Uri uri) => Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchMode launchMode) => Current.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => Current.OpenAsync(uri, options);

		public static IBrowser Current => ApplicationModel.Browser.Default;
	}
}
