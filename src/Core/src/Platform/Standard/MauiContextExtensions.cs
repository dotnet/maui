using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			return FlowDirection.LeftToRight;
		}
	}
}
