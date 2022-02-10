using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Specifies JavasScript to be evaluated by a platform web view control
	/// </summary>
	public class EvaluateJavaScriptAsyncRequest : TaskCompletionSource<string>
	{
		/// <summary>
		/// The JavaScript to be evaluated.
		/// </summary>
		public string Script { get; }

		/// <summary>
		/// Initializes a new instance of the EvaluateJavaScriptAsyncRequest class.
		/// </summary>
		/// <param name="script">The JavaScript to be evaluated.</param>
		public EvaluateJavaScriptAsyncRequest(string script)
		{
			Script = script;
		}
	}
}