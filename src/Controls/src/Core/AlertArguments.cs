#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Contains configuration arguments for displaying platform-specific alert dialogs.</summary>
	/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class AlertArguments
	{
		/// <summary>Initializes a new instance of the AlertArguments class with the specified dialog configuration.</summary>
		/// <param name="title">The title text to display in the alert dialog header.</param>
		/// <param name="message">The message text to display in the alert dialog body.</param>
		/// <param name="accept">The text for the positive/accept button in the alert dialog.</param>
		/// <param name="cancel">The text for the negative/cancel button in the alert dialog.</param>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public AlertArguments(string title, string message, string accept, string cancel)
		{
			Title = title;
			Message = message;
			Accept = accept;
			Cancel = cancel;
			Result = new TaskCompletionSource<bool>();
			FlowDirection = FlowDirection.MatchParent;
		}

		/// <summary>
		///     Gets the text for the accept button. Can be null.
		/// </summary>
		public string Accept { get; private set; }

		/// <summary>
		///     Gets the text of the cancel button.
		/// </summary>
		public string Cancel { get; private set; }

		/// <summary>
		///     Gets the message for the alert. Can be null.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>Gets the TaskCompletionSource used to handle the async result of the alert dialog interaction.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public TaskCompletionSource<bool> Result { get; }

		public FlowDirection FlowDirection { get; set; }

		/// <summary>
		///     Gets the title for the alert. Can be null.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>Sets the result of the alert dialog interaction.</summary>
		/// <param name="result">True if the user selected the accept button, false if they selected cancel or dismissed the dialog.</param>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public void SetResult(bool result)
		{
			Result.TrySetResult(result);
		}
	}
}
