using System.ComponentModel;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class AlertArguments
	{
		public AlertArguments(string title, string message, string accept, string cancel)
		{
			Title = title;
			Message = message;
			Accept = accept;
			Cancel = cancel;
			Result = new TaskCompletionSource<bool>();
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

		public TaskCompletionSource<bool> Result { get; }

		/// <summary>
		///     Gets the title for the alert. Can be null.
		/// </summary>
		public string Title { get; private set; }

		public void SetResult(bool result)
		{
			Result.TrySetResult(result);
		}
	}
}