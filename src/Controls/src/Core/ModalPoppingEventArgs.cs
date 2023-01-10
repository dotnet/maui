#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ModalPoppingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ModalPoppingEventArgs']/Docs/*" />
	public class ModalPoppingEventArgs : ModalEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ModalPoppingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ModalPoppingEventArgs(Page modal) : base(modal)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ModalPoppingEventArgs.xml" path="//Member[@MemberName='Cancel']/Docs/*" />
		public bool Cancel { get; set; }
	}
}