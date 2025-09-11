#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Arguments for the event that is raised when a modal window is popping from the navigation stack.</summary>
	public class ModalPoppingEventArgs : ModalEventArgs
	{
		/// <summary>Constructs a new <see cref="Microsoft.Maui.Controls.ModalPoppingEventArgs"/> object for the page that is being popped.</summary>
		/// <param name="modal">The modal page.</param>
		public ModalPoppingEventArgs(Page modal) : base(modal)
		{
		}

		/// <summary>Gets or sets a value that tells whether the modal navigation was canceled.</summary>
		public bool Cancel { get; set; }
	}
}