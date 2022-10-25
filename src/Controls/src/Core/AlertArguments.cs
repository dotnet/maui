using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/AlertArguments.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.AlertArguments']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class AlertArguments
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/AlertArguments.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/AlertArguments.xml" path="//Member[@MemberName='Result']/Docs/*" />
		public TaskCompletionSource<bool> Result { get; }

		public FlowDirection FlowDirection { get; set; }

		/// <summary>
		///     Gets the title for the alert. Can be null.
		/// </summary>
		public string Title { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/AlertArguments.xml" path="//Member[@MemberName='SetResult']/Docs/*" />
		public void SetResult(bool result)
		{
			Result.TrySetResult(result);
		}
	}
}
