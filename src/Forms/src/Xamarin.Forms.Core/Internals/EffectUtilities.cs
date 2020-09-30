using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectUtilities
	{
		public static void RegisterEffectControlProvider(IEffectControlProvider self, IElementController oldElement, IElementController newElement)
		{
			IElementController controller = oldElement;
			if (controller != null && controller.EffectControlProvider == self)
				controller.EffectControlProvider = null;

			controller = newElement;
			if (controller != null)
				controller.EffectControlProvider = self;
		}

		public static void UnregisterEffectControlProvider(IEffectControlProvider self, IElementController element)
		{
			if (element?.EffectControlProvider == self)
				element.EffectControlProvider = null;
		}
	}
}