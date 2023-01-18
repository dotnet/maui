using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls
{
	public static class IElementExtensions
	{
		internal static bool IsShimmed(this Microsoft.Maui.IElement? self)
		{
			string typeName = $"{self?.Handler?.GetType().Name}";

			return (typeName == "RendererToHandlerShim" ||
				typeName == "HandlerToRendererShim");
		}
	}
}