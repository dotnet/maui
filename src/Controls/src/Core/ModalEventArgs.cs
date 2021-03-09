using System;

namespace Microsoft.Maui.Controls
{
	public abstract class ModalEventArgs : EventArgs
	{
		protected ModalEventArgs(Page modal)
		{
			Modal = modal;
		}

		public Page Modal { get; private set; }
	}
}