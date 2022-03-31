#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	partial class BrowserImplementation : IBrowser
	{
		public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
