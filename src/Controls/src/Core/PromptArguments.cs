using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.PromptArguments']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class PromptArguments
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Message']/Docs" />
		public string Message { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Accept']/Docs" />
		public string Accept { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public string Cancel { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Placeholder']/Docs" />
		public string Placeholder { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='InitialValue']/Docs" />
		public string InitialValue { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='MaxLength']/Docs" />
		public int MaxLength { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Keyboard']/Docs" />
		public Keyboard Keyboard { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='Result']/Docs" />
		public TaskCompletionSource<string> Result { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/PromptArguments.xml" path="//Member[@MemberName='SetResult']/Docs" />
		public void SetResult(string text)
		{
			Result.TrySetResult(text);
		}
	}
}