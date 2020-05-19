using System;

namespace System.Maui
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