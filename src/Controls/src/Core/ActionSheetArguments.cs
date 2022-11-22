using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ActionSheetArguments.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.ActionSheetArguments']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionSheetArguments
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ActionSheetArguments.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ActionSheetArguments.xml" path="//Member[@MemberName='Result']/Docs/*" />
		public TaskCompletionSource<string> Result { get; }

		/// <summary>
		///     Gets the title for the action sheet. Can be null.
		/// </summary>
		public string Title { get; private set; }

		public FlowDirection FlowDirection { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ActionSheetArguments.xml" path="//Member[@MemberName='SetResult']/Docs/*" />
		public void SetResult(string result)
		{
			Result.TrySetResult(result);
		}
	}
}
