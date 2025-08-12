#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Base class for <see cref="T:Microsoft.Maui.Controls.ModalPushedEventArgs"/>, <see cref="T:Microsoft.Maui.Controls.ModalPushingEventArgs"/>, <see cref="T:Microsoft.Maui.Controls.ModalPoppedEventArgs"/>, and <see cref="T:Microsoft.Maui.Controls.ModalPoppingEventArgs"/>.</summary>
	public abstract class ModalEventArgs : EventArgs
	{
		protected ModalEventArgs(Page modal)
		{
			Modal = modal;
		}

		/// <summary>Gets or sets the page whose navigation triggered the event.</summary>
		public Page Modal { get; private set; }
	}
}