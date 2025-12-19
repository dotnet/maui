#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="Page.ModalAttemptedDismiss"/> event that fires when a user attempts to dismiss a modal page
	/// by interactive gestures (like swiping down on iOS FormSheet/PageSheet).
	/// </summary>
	public sealed class ModalAttemptedDismissEventArgs : EventArgs
	{
		internal ModalAttemptedDismissEventArgs()
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether the modal dismissal should be cancelled.
		/// Set to <see langword="true"/> to prevent the modal from being dismissed.
		/// </summary>
		public bool Cancel { get; set; }
	}
}
