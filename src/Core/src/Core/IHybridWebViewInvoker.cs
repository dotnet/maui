using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Abstraction for JS-to-.NET method invocation in HybridWebView.
	/// Implemented by the reflection-based fallback and by the source generator.
	/// </summary>
	public interface IHybridWebViewInvoker
	{
		/// <summary>
		/// Invokes the named .NET method with JSON-serialized parameters and returns a JSON-serialized result.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke.</param>
		/// <param name="paramJsonValues">JSON-serialized parameter values, or null for parameterless methods.</param>
		/// <returns>JSON-serialized result, or null for void methods or null returns.</returns>
		Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues);
	}
}
