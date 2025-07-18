#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Arguments for the event that is raised when a modal window is popped from the navigation stack.</summary>
	public class ModalPoppedEventArgs : ModalEventArgs
	{
		/// <summary>Constructs a new <see cref="Microsoft.Maui.Controls.ModalPoppedEventArgs"/> object for the page that was just popped.</summary>
		/// <param name="modal">The modal page.</param>
		public ModalPoppedEventArgs(Page modal) : base(modal)
		{
		}
	}
}