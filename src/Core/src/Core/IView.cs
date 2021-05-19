using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a visual element that is used to place layouts and controls on the screen.
	/// </summary>
	public interface IView : IFrameworkElement, IGestureController
	{
		/// <summary>
		/// The Margin represents the distance between an view and its adjacent views.
		/// </summary>
		Thickness Margin { get; }

		/// <summary>
		/// Include support to tap, pinch, pan, swipe, and drag and drop gestures on views.
		/// </summary>
		IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}