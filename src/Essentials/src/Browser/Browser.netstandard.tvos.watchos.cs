using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static partial class Browser
	{
		static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
