using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides AOT-friendly dispatch of .NET methods invoked from JavaScript in a HybridWebView.
	/// Implement this interface directly for full control, or use the
	/// <see cref="HybridWebViewDotNetMethodProviderAttribute"/> with a source generator.
	/// </summary>
	public interface IHybridWebViewDotNetMethodProvider
	{
		/// <summary>
		/// Invokes a .NET method with the specified name and JSON-serialized parameters.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke.</param>
		/// <param name="paramJsonValues">
		/// An array of JSON-serialized parameter values, or <c>null</c> if the method takes no parameters.
		/// Each element is a valid JSON string representing one parameter.
		/// </param>
		/// <returns>
		/// A JSON-serialized result string, or <c>null</c> for void methods or methods that return <c>null</c>.
		/// </returns>
		Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues);
	}
}
