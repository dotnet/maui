#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Arguments for the event that is raised when a modal window is being pushed onto the navigation stack.</summary>
	public class ModalPushingEventArgs : ModalEventArgs
	{
		/// <summary>Constructs a new <see cref="Microsoft.Maui.Controls.ModalPushingEventArgs"/> object for the page that is being pushed.</summary>
		/// <param name="modal">The modal page.</param>
		public ModalPushingEventArgs(Page modal) : base(modal)
		{
		}
	}
}