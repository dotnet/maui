using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Interface for platform-specific gesture management.
	/// Allows users to provide custom implementations to replace the default gesture handling behavior.
	/// </summary>
	internal interface IGesturePlatformManager : IDisposable
	{
	}
}
