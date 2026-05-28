using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Interface for platform-specific gesture management.
	/// Allows users to provide custom implementations to replace the default gesture handling behavior.
	/// </summary>
	/// <remarks>
	/// Instances are created by <see cref="IGesturePlatformManagerFactory"/> and owned by
	/// <see cref="GestureManager"/>, which disposes the instance when the handler disconnects.
	/// </remarks>
	internal interface IGesturePlatformManager : IDisposable
	{
	}
}
