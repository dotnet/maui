using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ModalEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ModalEventArgs']/Docs" />
	public abstract class ModalEventArgs : EventArgs
	{
		protected ModalEventArgs(Page modal)
		{
			Modal = modal;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ModalEventArgs.xml" path="//Member[@MemberName='Modal']/Docs" />
		public Page Modal { get; private set; }
	}
}