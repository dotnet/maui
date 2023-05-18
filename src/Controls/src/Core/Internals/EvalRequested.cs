#nullable disable
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public delegate Task<string> EvaluateJavaScriptDelegate(string script);

	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EvalRequested.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.EvalRequested']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EvalRequested : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EvalRequested.xml" path="//Member[@MemberName='Script']/Docs/*" />
		public string Script { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/EvalRequested.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public EvalRequested(string script)
		{
			Script = script;
		}
	}
}