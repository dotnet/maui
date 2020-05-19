
#if __MOBILE__
namespace System.Maui.Platform.iOS
#else

namespace System.Maui.Platform.MacOS
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
			if (controller != null)
				controller.EffectControlProvider = self;
		}
	}
}