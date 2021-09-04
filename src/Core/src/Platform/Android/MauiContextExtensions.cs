using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var config = mauiContext?.Context?.Resources?.Configuration;
			if (config == null)
				return FlowDirection.LeftToRight;

			return config.LayoutDirection.ToFlowDirection();
		}
	}
}
