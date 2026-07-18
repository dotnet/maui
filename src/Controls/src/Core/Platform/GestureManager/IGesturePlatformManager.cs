using System;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Manages the platform-specific gesture infrastructure for a single handler connection.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The framework creates an instance whenever a handler connects to a view and disposes it when
	/// the handler disconnects or changes. A new instance is created for every connection, so
	/// implementations must be prepared for multiple connect/disconnect cycles.
	/// </para>
	/// <para>
	/// To replace the built-in gesture handling for all views in an application, register a custom
	/// <see cref="IGesturePlatformManagerFactory"/> in the application's service collection. The
	/// framework also supports an internal handler-scoped customization path.
	/// </para>
	/// </remarks>
	public interface IGesturePlatformManager : IDisposable
	{
	}
}
