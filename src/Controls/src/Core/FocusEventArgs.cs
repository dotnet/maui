#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event args for <see cref="Microsoft.Maui.Controls.VisualElement"/>'s <see cref="Microsoft.Maui.Controls.VisualElement.Focused"/> and <see cref="Microsoft.Maui.Controls.VisualElement.Unfocused"/> events.</summary>
	public class FocusEventArgs : EventArgs
	{
		/// <summary>Constructs and initializes a new instance of the <see cref="Microsoft.Maui.Controls.FocusEventArgs"/> class.</summary>
		/// <param name="visualElement">The <see cref="Microsoft.Maui.Controls.VisualElement"/> whose focused was changed.</param>
		/// <param name="isFocused">Whether or not the <paramref name="visualElement"/> was focused.</param>
		public FocusEventArgs(VisualElement visualElement, bool isFocused)
		{
			if (visualElement == null)
				throw new ArgumentNullException(nameof(visualElement));

			VisualElement = visualElement;
			IsFocused = isFocused;
		}

		/// <summary>Gets whether or not the <see cref="Microsoft.Maui.Controls.FocusEventArgs.VisualElement"/> was focused.</summary>
		public bool IsFocused { get; private set; }

		/// <summary>Gets the <see cref="Microsoft.Maui.Controls.VisualElement"/> who's focused was changed.</summary>
		public VisualElement VisualElement { get; private set; }
	}
}