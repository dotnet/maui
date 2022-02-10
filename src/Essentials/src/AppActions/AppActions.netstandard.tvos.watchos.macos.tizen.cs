using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	public static partial class AppActions
	{
		internal static bool PlatformIsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<IEnumerable<AppAction>> PlatformGetAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformSetAsync(IEnumerable<AppAction> actions) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
