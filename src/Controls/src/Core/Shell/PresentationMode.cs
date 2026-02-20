using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies how pages are presented during navigation.</summary>
	[Flags]
	public enum PresentationMode
	{
		/// <summary>Navigation occurs without animation.</summary>
		NotAnimated = 1,
		/// <summary>Navigation occurs with animation.</summary>
		Animated = 1 << 1,
		/// <summary>Page is presented modally.</summary>
		Modal = 1 << 2,
		/// <summary>Page is presented modally with animation.</summary>
		ModalAnimated = PresentationMode.Animated | PresentationMode.Modal,
		/// <summary>Page is presented modally without animation.</summary>
		ModalNotAnimated = PresentationMode.NotAnimated | PresentationMode.Modal
	}
}
