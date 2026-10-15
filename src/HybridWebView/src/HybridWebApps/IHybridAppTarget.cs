namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>The only method exposed to JavaScript by the host's generated invocation target.</summary>
public interface IHybridAppTarget
{
	/// <summary>Dispatches an ASCII-escaped protocol request.</summary>
	/// <param name="request">The protocol-1 request JSON string.</param>
	/// <returns>The response JSON string, including explicit error or cancellation status.</returns>
	Task<string> Invoke(string request);
}
