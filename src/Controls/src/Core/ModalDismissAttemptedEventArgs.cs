#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Arguments for the event that is raised when a user attempts to dismiss a modal window but the dismissal is prevented.</summary>
	public class ModalDismissAttemptedEventArgs : ModalEventArgs
	{
		/// <summary>Constructs a new <see cref="Microsoft.Maui.Controls.ModalDismissAttemptedEventArgs"/> object for the modal page that the user attempted to dismiss.</summary>
		/// <param name="modal">The modal page.</param>
		public ModalDismissAttemptedEventArgs(Page modal) : base(modal)
		{
		}
	}
}
