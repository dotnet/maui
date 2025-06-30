#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class AlertArguments
	{
		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="title">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="message">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="accept">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <param name="cancel">For internal use by the Microsoft.Maui.Controls platform.</param>
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

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		public TaskCompletionSource<bool> Result { get; }

		public FlowDirection FlowDirection { get; set; }

		/// <summary>
		///     Gets the title for the alert. Can be null.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="result">For internal use by the Microsoft.Maui.Controls platform.</param>
		public void SetResult(bool result)
		{
			Result.TrySetResult(result);
		}
	}
}
