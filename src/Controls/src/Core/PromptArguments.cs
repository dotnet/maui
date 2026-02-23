#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Arguments for a prompt dialog.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class PromptArguments
	{
		/// <summary>Creates a new <see cref="PromptArguments"/> with the specified parameters.</summary>
		public PromptArguments(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default(Keyboard), string initialValue = "")
		{
			Title = title;
			Message = message;
			Accept = accept;
			Cancel = cancel;
			Placeholder = placeholder;
			InitialValue = initialValue;
			MaxLength = maxLength;
			Keyboard = keyboard ?? Keyboard.Default;
			Result = new TaskCompletionSource<string>();
		}

		/// <summary>Gets the title for the prompt.</summary>
		public string Title { get; }

		/// <summary>Gets the message for the prompt.</summary>
		public string Message { get; }

		/// <summary>Gets the text for the accept button.</summary>
		public string Accept { get; }

		/// <summary>Gets the text for the cancel button.</summary>
		public string Cancel { get; }

		/// <summary>Gets the placeholder text for the input field.</summary>
		public string Placeholder { get; }

		/// <summary>Gets the initial value for the input field.</summary>
		public string InitialValue { get; }

		/// <summary>Gets the maximum input length, or -1 for no limit.</summary>
		public int MaxLength { get; }

		/// <summary>Gets the keyboard type for the input field.</summary>
		public Keyboard Keyboard { get; }

		/// <summary>Gets the task completion source for the user's input.</summary>
		public TaskCompletionSource<string> Result { get; }

		/// <summary>Sets the result of the prompt.</summary>
		/// <param name="text">The user's input text.</param>
		public void SetResult(string text)
		{
			Result.TrySetResult(text);
		}
	}
}