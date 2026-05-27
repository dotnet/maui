#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Utility methods for managing effect control providers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectUtilities
	{
		/// <summary>Registers an effect control provider with an element.</summary>
		public static void RegisterEffectControlProvider(IEffectControlProvider self, IElementController oldElement, IElementController newElement)
		{
			IElementController controller = oldElement;
			if (controller != null && controller.EffectControlProvider == self)
				controller.EffectControlProvider = null;

			controller = newElement;
			if (controller != null)
				controller.EffectControlProvider = self;
		}

		/// <summary>Unregisters an effect control provider from an element.</summary>
		public static void UnregisterEffectControlProvider(IEffectControlProvider self, IElementController element)
		{
			if (element?.EffectControlProvider == self)
				element.EffectControlProvider = null;
		}
	}
}