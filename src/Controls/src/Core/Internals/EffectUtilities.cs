#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EffectUtilities.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.EffectUtilities']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class EffectUtilities
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EffectUtilities.xml" path="//Member[@MemberName='RegisterEffectControlProvider']/Docs/*" />
		public static void RegisterEffectControlProvider(IEffectControlProvider self, IElementController oldElement, IElementController newElement)
		{
			IElementController controller = oldElement;
			if (controller != null && controller.EffectControlProvider == self)
				controller.EffectControlProvider = null;

			controller = newElement;
			controller?.EffectControlProvider = self;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EffectUtilities.xml" path="//Member[@MemberName='UnregisterEffectControlProvider']/Docs/*" />
		public static void UnregisterEffectControlProvider(IEffectControlProvider self, IElementController element)
		{
			if (element?.EffectControlProvider == self)
				element.EffectControlProvider = null;
		}
	}
}