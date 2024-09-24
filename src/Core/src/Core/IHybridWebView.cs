using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IHybridWebView : IView
	{
		/// <summary>
		/// Specifies the file within the <see cref="HybridRoot"/> that should be served as the default file. The
		/// default value is <c>index.html</c>.
		/// </summary>
		string? DefaultFile { get; }

		/// <summary>
		///  The path within the app's "Raw" asset resources that contain the web app's contents. For example, if the
		///  files are located in <c>[ProjectFolder]/Resources/Raw/hybrid_root</c>, then set this property to "hybrid_root".
		///  The default value is <c>wwwroot</c>, which maps to <c>[ProjectFolder]/Resources/Raw/wwwroot</c>.
		/// </summary>
		string? HybridRoot { get; }

		void MessageReceived(string rawMessage);

		void SendRawMessage(string rawMessage);

		/// <summary>
		/// Runs the JavaScript code provided in the <paramref name="script"/> parameter and returns the result as a string.
		/// </summary>
		/// <param name="script">The JavaScript code to run.</param>
		/// <returns>The return value (if any) of running the script.</returns>
		Task<string?> EvaluateJavaScriptAsync(string script);
	}
}
