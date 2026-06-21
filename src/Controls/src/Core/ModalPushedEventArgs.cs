#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Arguments for the event that is raised when a modal window is pushed onto the navigation stack.</summary>
	public class ModalPushedEventArgs : ModalEventArgs
	{
		/// <summary>Constructs a new <see cref="Microsoft.Maui.Controls.ModalPushedEventArgs"/> object for the page that was just popped.</summary>
		/// <param name="modal">The modal page.</param>
		public ModalPushedEventArgs(Page modal) : base(modal)
		{
		}
	}
}