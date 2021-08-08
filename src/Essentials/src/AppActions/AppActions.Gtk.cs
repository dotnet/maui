using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
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
