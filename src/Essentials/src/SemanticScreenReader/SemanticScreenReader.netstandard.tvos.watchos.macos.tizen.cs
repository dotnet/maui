using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
