using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var window = mauiContext.Window;
			if (window == null)
				return FlowDirection.LeftToRight;

			return window.EffectiveUserInterfaceLayoutDirection.ToFlowDirection();
		}
	}
}
