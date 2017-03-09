using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionSheetArguments
	{
		public ActionSheetArguments(string title, string cancel, string destruction, IEnumerable<string> buttons)
		{
			Title = title;
			Cancel = cancel;
			Destruction = destruction;
			Buttons = buttons;
			Result = new TaskCompletionSource<string>();
		}

		/// <summary>
		///     Gets titles of any buttons on the action sheet that aren't <see cref="Cancel" /> or <see cref="Destruction" />. Can
		///     be <c>null</c>.
		/// </summary>
		public IEnumerable<string> Buttons { get; private set; }

		/// <summary>
		///     Gets the text for a cancel button. Can be null.
		/// </summary>
		public string Cancel { get; private set; }

		/// <summary>
		///     Gets the text for a destructive button. Can be null.
		/// </summary>
		public string Destruction { get; private set; }

		public TaskCompletionSource<string> Result { get; }

		/// <summary>
		///     Gets the title for the action sheet. Can be null.
		/// </summary>
		public string Title { get; private set; }

		public void SetResult(string result)
		{
			Result.TrySetResult(result);
		}
	}
}