#if !(__ANDROID__ || __IOS__ || WINDOWS)
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class Forms
	{
		[Obsolete]
		public static void Init(IActivationState activationState)
		{
			throw new NotImplementedException();
		}

		internal static IMauiContext MauiContext => throw new NotImplementedException();
	}
}
#endif
