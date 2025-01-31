using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers;

abstract class HybridWebViewInvokeJavaScriptTargetManager
{
	public HybridWebViewInvokeJavaScriptTargetManager(object invokeTarget)
	{
		InvokeTarget = invokeTarget;
	}

	public object InvokeTarget { get; }

	/// <summary>
	/// Invokes a .NET method on the JavaScript target object using the specified method name and parameter values.
	/// The parameters will be deserialized from JSON and the return value (if any) will be serialized back to JSON.
	/// </summary>
	/// <param name="methodName">The name of the method on the target object to invoke.</param>
	/// <param name="parameterValues">The optional, JSON-serialized parameter values to use when invoking the method.</param>
	/// <returns>Returns the JSON-serialized value that the method returned; or null if the method was a void return.</returns>
	public abstract Task<string?> InvokeDotNetMethodAsync(string methodName, string[]? parameterValues);
}
