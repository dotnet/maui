#nullable enable

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class GestureManager
	{
		static partial void IsPlatformHandlerCore(IElementHandler handler, ref bool result)
		{
			if (handler is not IPlatformViewHandler)
				result = false;
		}
	}
}
