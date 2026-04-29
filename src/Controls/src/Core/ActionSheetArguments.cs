#nullable disable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Arguments for an action sheet dialog.</summary>
	/// <remarks>
	/// Third-party platform backends may supply a custom action sheet implementation by registering
	/// a keyed <see cref="System.Func{T1, T2, TResult}"/> of
	/// <c>Func&lt;Microsoft.Maui.Controls.Page, ActionSheetArguments, System.Threading.Tasks.Task&lt;string&gt;&gt;</c>
	/// in the application's <see cref="System.IServiceProvider"/> with the service key
	/// <c>Microsoft.Maui.Controls.DisplayActionSheet</c>. The delegate should return the selected
	/// button text, or <see langword="null"/> when the action sheet is canceled or dismissed. Unkeyed
	/// delegates are not considered by this convention.
	/// <para>
	/// Note: the keyed registration is reserved for MAUI action sheet handling. Do not reuse this
	/// service key for unrelated services in the same service collection.
	/// </para>
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionSheetArguments
	{
		/// <summary>Creates a new <see cref="ActionSheetArguments"/> with the specified parameters.</summary>
		/// <param name="title">The title for the action sheet.</param>
		/// <param name="cancel">The text for the cancel button.</param>
		/// <param name="destruction">The text for the destructive button.</param>
		/// <param name="buttons">Additional buttons to display.</param>
		public ActionSheetArguments(string title, string cancel, string destruction, IEnumerable<string> buttons)
		{
			Title = title;
			Cancel = cancel;
			Destruction = destruction;
			Buttons = buttons?.Where(c => c != null);
			Result = new TaskCompletionSource<string>();
			FlowDirection = FlowDirection.MatchParent;
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

		/// <summary>Gets the task completion source for the user's choice.</summary>
		public TaskCompletionSource<string> Result { get; }

		/// <summary>
		///     Gets the title for the action sheet. Can be null.
		/// </summary>
		public string Title { get; private set; }

		public FlowDirection FlowDirection { get; set; }

		/// <summary>Sets the result of the action sheet.</summary>
		/// <param name="result">The selected button text.</param>
		public void SetResult(string result)
		{
			Result.TrySetResult(result);
		}
	}
}
