
#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	internal static class EffectUtilities
	{
		public static void RegisterEffectControlProvider(IEffectControlProvider self, IElementController oldElement,
			IElementController newElement)
		{
			IElementController controller = oldElement;
			if (controller != null && controller.EffectControlProvider == self)
				controller.EffectControlProvider = null;

			controller = newElement;
			controller?.EffectControlProvider = self;
		}
	}
}