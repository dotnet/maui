#nullable disable
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public delegate Task<string> EvaluateJavaScriptDelegate(string script);

	/// <summary>Event args for JavaScript evaluation requests in WebView.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EvalRequested : EventArgs
	{
		/// <summary>Gets the JavaScript to evaluate.</summary>
		public string Script { get; }

		/// <summary>Creates a new EvalRequested with the specified script.</summary>
		public EvalRequested(string script)
		{
			Script = script;
		}
	}
}